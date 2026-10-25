using Microsoft.Data.SqlClient;
using ShopAPI.DTOClasses;
using System.Data;

namespace ShopAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;
        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ShopDbConnection");
        }

        public async Task<List<ProductDTO>> GetAllAsync()
        {
            List<ProductDTO> listproduct = new List<ProductDTO>();
            string sql = "Select Product_ID, ProductName, Description, UnitPrice, Category_ID, RemainingQuantity, IsActive From Products;";
            using (SqlConnection con = new SqlConnection(_connectionString))
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
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo._SP_AddNewProduct", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar, 150).Value = product.ProductName;
                    cmd.Parameters.Add("@Des", SqlDbType.NVarChar, 1000).Value = product.Description;
                    cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = product.UnitPrice;
                    cmd.Parameters.Add("@Category_ID", SqlDbType.Int).Value = product.CategoryId;

                    var newId = cmd.Parameters.Add("@New_ID", SqlDbType.Int);
                    newId.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return (int)(newId.Value ?? 0);
                }
            }
        }

        public async Task<ProductDTO> GetByIdAsync(int id)
        {
            string sql = "Select Product_ID, ProductName, Description, UnitPrice, Category_ID, RemainingQuantity, IsActive From Products where Product_ID = @Product_ID";
            using (SqlConnection con = new SqlConnection(_connectionString))
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.UpdateProduct", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value = updateProductDTO.Description;
                    cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = updateProductDTO.UnitPrice;
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
            using (SqlConnection con = new SqlConnection(_connectionString))
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
            string sql = "UPDATE Products SET IsActive = 0 Where Product_ID = @Product_ID; SELECT @@ROWCOUNT";
            using (SqlConnection con = new SqlConnection(_connectionString))
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
            using (SqlConnection con = new SqlConnection(_connectionString))
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
