using Microsoft.Data.SqlClient;
using ShopAPI.DTOClasses;
using ShopAPI.Infrastructure;
using System.Data;

namespace ShopAPI.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _db;

    public CategoryRepository(IDbConnectionFactory db) => _db = db;

    public async Task<List<CategoryDTO>> GetAllAsync()
    {
        const string sql = "SELECT Category_ID, CategoryName FROM dbo.Categories ORDER BY CategoryName";
        var result = new List<CategoryDTO>();
        await using var connection = _db.CreateConnection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new CategoryDTO { CategoryId = reader.GetInt32(0), CategoryName = reader.GetString(1)});
        }
        return result;
    }

    public async Task<CategoryDTO?> GetByIdAsync(int id)
    {
        const string sql = "SELECT Category_ID, CategoryName FROM dbo.Categories WHERE Category_ID = @CategoryId";
        await using var connection = _db.CreateConnection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return new CategoryDTO { CategoryId = reader.GetInt32(0), CategoryName = reader.GetString(1) };
    }

    public async Task<int?> CreateAsync(string categoryName)
    {
        const string sql = "INSERT INTO dbo.Categories (CategoryName) OUTPUT INSERTED.Category_ID VALUES (@CategoryName)";
        await using var connection = _db.CreateConnection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100).Value = categoryName;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateAsync(int id, string categoryName)
    {
        const string sql = "UPDATE dbo.Categories SET CategoryName = @CategoryName WHERE Category_ID = @CategoryId";
        await using var connection = _db.CreateConnection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100).Value = categoryName;
        command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = id;
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM dbo.Categories WHERE Category_ID = @CategoryId";
        await using var connection = _db.CreateConnection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = id;
        return await command.ExecuteNonQueryAsync() > 0;
    }
}
