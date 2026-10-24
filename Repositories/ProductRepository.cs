using Microsoft.Data.SqlClient;
using ShopAPI.DTOClasses;
using ShopAPI.Infrastructure;
using ShopAPI.Response;
using System.Data;

namespace ShopAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnectionFactory _db;
        public ProductRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<List<ProductDTO>> GetAllAsync()
        {
            List<ProductDTO> listproduct = new List<ProductDTO>();
            string sql = "Select Product_ID, ProductName, Description, UnitPrice, Category_ID, RemainingQuantity, IsActive From Products;";
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            listproduct.Add(MapProduct(reader));
                        }
                    }
                }
            }
            return listproduct;
        }

        public async Task<int> AddNewAsync(CreateProductDTO product)
        {
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo._SP_AddNewProduct", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar, 150).Value = product.ProductName;
                    cmd.Parameters.Add("@Des", SqlDbType.NVarChar, 1000).Value = product.Description;
                    var price = cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal);
                    price.Precision = 18;
                    price.Scale = 2;
                    price.Value = product.UnitPrice;
                    cmd.Parameters.Add("@Category_ID", SqlDbType.Int).Value = product.CategoryId;

                    var newId = cmd.Parameters.Add("@New_ID", SqlDbType.Int);
                    newId.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return (int)(newId.Value ?? 0);
                }
            }
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            string sql = "Select Product_ID, ProductName, Description, UnitPrice, Category_ID, RemainingQuantity, IsActive From Products where Product_ID = @Product_ID";
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@Product_ID", SqlDbType.Int).Value = id;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync()) return null;
                        return MapProduct(reader);
                    }
                }
            }
        }

        public async Task<bool> UpdateAsync(UpdateProductDTO updateProductDTO, int id)
        {
            using (SqlConnection conn = _db.CreateConnection())
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.UpdateProduct", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value = updateProductDTO.Description;
                    var price = cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal);
                    price.Precision = 18;
                    price.Scale = 2;
                    price.Value = updateProductDTO.UnitPrice;
                    cmd.Parameters.Add("@Product_ID", SqlDbType.Int).Value = id;
                    var rowsaff = cmd.Parameters.Add("@RowsAffected", SqlDbType.Int);
                    rowsaff.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return ((int)(rowsaff.Value ?? 0)) > 0;
                }
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.DeleteProduct", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Product_ID", SqlDbType.Int).Value = id;
                    var rowsaff = cmd.Parameters.Add("@RowsAffected", SqlDbType.Int);
                    rowsaff.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return ((int)(rowsaff.Value ?? 0)) > 0;
                }
            }
        }

        public async Task<bool> DisAbleAsync(int id)
        {
            string sql = "UPDATE Products SET IsActive = 0 WHERE Product_ID = @Product_ID AND IsActive = 1; SELECT @@ROWCOUNT";
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@Product_ID", SqlDbType.Int).Value = id;
                    int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return result > 0;
                }
            }
        }

        public async Task<List<ProductDTO>> GetActiveAsync()
        {
            List<ProductDTO> listproduct = new List<ProductDTO>();
            string sql = "Select Product_ID, ProductName, Description, UnitPrice, Category_ID, RemainingQuantity, IsActive From Products Where IsActive = 1;";
            using (SqlConnection con = _db.CreateConnection())
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            listproduct.Add(MapProduct(reader));
                        }
                    }
                }
            }
            return listproduct;
        }
        public async Task<PagedResult<ProductDTO>> GetActivePageAsync(int page, int pageSize)
        {
            var offset = ((long)page - 1) * pageSize;

            const string sql = @"SELECT COUNT(*) AS TotalCount FROM dbo.Products WHERE IsActive = 1;
            SELECT
            Product_ID,
            ProductName,
            Description,
            UnitPrice,
            Category_ID,
            RemainingQuantity,
            IsActive
            FROM dbo.Products
            WHERE IsActive = 1
            ORDER BY Product_ID
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;";

            var items = new List<ProductDTO>();
            var total = 0;

            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Offset", SqlDbType.BigInt).Value = offset;
            command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                total = reader.GetInt32(0);

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
                items.Add(MapProduct(reader));

            return new PagedResult<ProductDTO>{Items = items,Page = page,PageSize = pageSize,TotalCount = total};
        }

        public async Task<bool> RestockAsync(int productId, int quantity, string reason)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();
            using var command = new SqlCommand("dbo.SP_RestockProduct", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@Product_ID", SqlDbType.Int).Value = productId;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
            command.Parameters.Add("@Reason", SqlDbType.NVarChar, 200).Value = reason;
            var rows = command.Parameters.Add("@RowsAffected", SqlDbType.Int);
            rows.Direction = ParameterDirection.Output;
            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(rows.Value) > 0;
        }
        private static ProductDTO MapProduct(SqlDataReader reader)
        {
            return new ProductDTO
            {
                ProductId = Convert.ToInt32(reader["Product_ID"]),
                ProductName = reader["ProductName"].ToString(),
                Description = reader["Description"].ToString(),
                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                CategoryId = Convert.ToInt32(reader["Category_ID"]),
                RemainingQuantity = Convert.ToInt32(reader["RemainingQuantity"]),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }
    }
}
