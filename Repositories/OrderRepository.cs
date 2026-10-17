using Microsoft.Data.SqlClient;
using ShopAPI.DTOClasses;
using ShopAPI.Infrastructure;
using ShopAPI.Response;
using System.Data;

namespace ShopAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnectionFactory _db;
        public OrderRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<List<OrderDTO>> GetAllAsync()
        {
            string sql =
                "SELECT o.Order_ID, o.CreatedAt, o.DeliveredAt, o.Status, " +
                "       oi.OrderItem_ID, oi.Product_ID, p.ProductName, oi.Quantity, oi.UnitPrice, oi.TotalPrice " +
                "FROM dbo.Orders o " +
                "LEFT JOIN dbo.OrderItems oi ON oi.Order_ID = o.Order_ID " +
                "LEFT JOIN dbo.Products p ON p.Product_ID = oi.Product_ID " +
                "ORDER BY o.Order_ID;";

            var orderDictionary = new Dictionary<int, OrderDTO>();

            using (var connection = _db.CreateConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        MapOrderRow(reader, orderDictionary);
                    }
                }
            }

            return orderDictionary.Values.ToList();
        }

        public async Task<List<OrderDTO>> GetByPersonIdAsync(int personId)
        {
            string sql =
                "SELECT o.Order_ID, o.CreatedAt, o.DeliveredAt, o.Status, " +
                "       oi.OrderItem_ID, oi.Product_ID, p.ProductName, oi.Quantity, oi.UnitPrice, oi.TotalPrice " +
                "FROM dbo.Orders o " +
                "LEFT JOIN dbo.OrderItems oi ON oi.Order_ID = o.Order_ID " +
                "LEFT JOIN dbo.Products p ON p.Product_ID = oi.Product_ID " +
                "WHERE o.Person_ID = @PersonId " +
                "ORDER BY o.CreatedAt DESC, o.Order_ID DESC;";
            var orderDictionary = new Dictionary<int, OrderDTO>();
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) MapOrderRow(reader, orderDictionary);
            return orderDictionary.Values.ToList();
        }

        public async Task<PagedResult<OrderDTO>> GetByPersonPageAsync(int personId,int page,int pageSize)
        {
            long offset = ((long)page - 1) * pageSize;
            const string sql = @"SELECT COUNT(*) AS TotalCount FROM dbo.Orders WHERE Person_ID = @PersonId; ;WITH PagedOrders AS
            (
             SELECT Order_ID, CreatedAt, DeliveredAt, Status
             FROM dbo.Orders
             WHERE Person_ID = @PersonId
             ORDER BY CreatedAt DESC, Order_ID DESC
             OFFSET @Offset ROWS
             FETCH NEXT @PageSize ROWS ONLY
            )
            SELECT
            o.Order_ID,
            o.CreatedAt,
            o.DeliveredAt,
            o.Status,
            oi.OrderItem_ID,
            oi.Product_ID,
            p.ProductName,
            oi.Quantity,
            oi.UnitPrice,
            oi.TotalPrice
            FROM PagedOrders o
            LEFT JOIN dbo.OrderItems oi ON oi.Order_ID = o.Order_ID
            LEFT JOIN dbo.Products p ON p.Product_ID = oi.Product_ID
            ORDER BY o.CreatedAt DESC, o.Order_ID DESC;";

            var orders = new Dictionary<int, OrderDTO>();
            var total = 0;

            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
            command.Parameters.Add("@Offset", SqlDbType.BigInt).Value = offset;
            command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                total = reader.GetInt32(0);

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
                MapOrderRow(reader, orders);

            return new PagedResult<OrderDTO>{Items = orders.Values.ToList(),Page = page,PageSize = pageSize,TotalCount = total};
        }

        public async Task<OrderDTO?> GetByIdAsync(int id)
        {
            string sql =
                "SELECT o.Order_ID, o.CreatedAt, o.DeliveredAt, o.Status, " +
                "       oi.OrderItem_ID, oi.Product_ID, p.ProductName, oi.Quantity, oi.UnitPrice, oi.TotalPrice " +
                "FROM dbo.Orders o " +
                "LEFT JOIN dbo.OrderItems oi ON oi.Order_ID = o.Order_ID " +
                "LEFT JOIN dbo.Products p ON p.Product_ID = oi.Product_ID " +
                "WHERE o.Order_ID = @OrderId;";

            var orderDictionary = new Dictionary<int, OrderDTO>();

            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = id;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            MapOrderRow(reader, orderDictionary);
                        }
                    }
                }
            }

            return orderDictionary.TryGetValue(id, out var order) ? order : null;
        }

        public Task<int> AddNewAsync(CreateOrderDTO order, int personId)
            => AddNewAsync(order, personId, Guid.NewGuid().ToString("N"), IdempotencyKeyHasher.Compute(order));

        public async Task<int> AddNewAsync(CreateOrderDTO order, int personId, string idempotencyKey, string requestHash)
        {
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.AddListOfOrderItemToOrder", con))
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Product_ID", typeof(int));
                    dt.Columns.Add("Quantity", typeof(int));
                    dt.Columns.Add("UnitPrice", typeof(decimal));

                    foreach (var oi in order.Items)
                    {
                        dt.Rows.Add(oi.ProductId, oi.Quantity, DBNull.Value);
                    }

                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@T", dt);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.OrderTableType";

                    cmd.Parameters.Add("@Person_ID", SqlDbType.Int).Value = personId;
                    cmd.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = DBNull.Value;
                    cmd.Parameters.Add("@IdempotencyKey", SqlDbType.NVarChar, 100).Value = idempotencyKey;
                    cmd.Parameters.Add("@RequestHash", SqlDbType.Char, 64).Value = requestHash;

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateAsync(UpdateOrderDTO order, int orderId)
        {
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SP_UpdateOrders", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Order_Id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@D_At", SqlDbType.DateTime2).Value = (object?)order.DeliveredAt ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = order.Status;

                    var rowsAffectedParam = cmd.Parameters.Add("@RowsAffected", SqlDbType.Int);
                    rowsAffectedParam.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return ((int)(rowsAffectedParam.Value ?? 0)) > 0;
                }
            }
        }

        public Task<bool> DeleteAsync(int id) => CancelAsync(id);

        public async Task<bool> CancelAsync(int id)
        {
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.SP_CancelOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Order_ID", SqlDbType.Int).Value = id;

                    var rowsAffectedParam = cmd.Parameters.Add("@RowsAffected", SqlDbType.Int);
                    rowsAffectedParam.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return ((int)(rowsAffectedParam.Value ?? 0)) > 0;
                }
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Orders WHERE Order_ID = @OrderId", con))
                {
                    cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = id;
                    int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task<int> GetQuantity(int productId)
        {
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.SP_GetRemainingQuantity", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@productId", SqlDbType.Int).Value = productId;
                    var result = await cmd.ExecuteScalarAsync();
                    return result == null || result is DBNull ? 0 : Convert.ToInt32(result);
                }
            }
        }

        private static void MapOrderRow(SqlDataReader reader, Dictionary<int, OrderDTO> orderDictionary)
        {
            int orderId = Convert.ToInt32(reader["Order_ID"]);
            if (!orderDictionary.TryGetValue(orderId, out OrderDTO order))
            {
                order = new OrderDTO
                {
                    OrderId = orderId,
                    CreatedAt = reader["CreatedAt"] is DBNull ? null : Convert.ToDateTime(reader["CreatedAt"]),
                    DeliveredAt = reader["DeliveredAt"] is DBNull ? null : Convert.ToDateTime(reader["DeliveredAt"]),
                    Status = reader["Status"].ToString()
                };
                orderDictionary.Add(orderId, order);
            }

            if (reader["OrderItem_ID"] is DBNull) return;

            order.Items.Add(new OrderItemsDTO
            {
                OrderItemId = Convert.ToInt32(reader["OrderItem_ID"]),
                ProductId = Convert.ToInt32(reader["Product_ID"]),
                ProductName = reader["ProductName"].ToString(),
                Quantity = Convert.ToInt32(reader["Quantity"]),
                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                TotalPrice = Convert.ToDecimal(reader["TotalPrice"])
            });
        }

        public async Task<int?> GetOwnerIdAsync(int orderId)
        {
            using var connection = _db.CreateConnection();
            using var command = new SqlCommand("GetOrderOwnerId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@OrderId", orderId);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
        }
    }
}
