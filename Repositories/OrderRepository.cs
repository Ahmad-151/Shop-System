using Microsoft.Data.SqlClient;
using ShopAPI.DTOClasses;
using System.Data;

namespace ShopAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;
        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ShopDbConnection");
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

            using (var connection = new SqlConnection(_connectionString))
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

            using (SqlConnection con = new SqlConnection(_connectionString))
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

        public async Task<int> AddNewAsync(CreateOrderDTO order, int personId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
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
                    cmd.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value =
                        (object?)order.CreatedAt ?? DBNull.Value;

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateAsync(UpdateOrderDTO order, int orderId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
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

        public async Task<bool> DeleteAsync(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.DeleteOrder_ItsItems", con))
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
            using (SqlConnection con = new SqlConnection(_connectionString))
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
            using (SqlConnection con = new SqlConnection(_connectionString))
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
            using var connection = new SqlConnection(_connectionString);
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
