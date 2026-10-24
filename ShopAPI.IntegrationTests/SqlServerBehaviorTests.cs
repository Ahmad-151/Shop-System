using Microsoft.Data.SqlClient;
using System.Data;
using Xunit.Sdk;

namespace ShopAPI.IntegrationTests;

public sealed class SqlServerBehaviorTests
{
    private static string ConnectionString => Environment.GetEnvironmentVariable("SHOP_SQL_CONNECTION") ?? string.Empty;

    [SqlServerFact]
    public async Task ConcurrentOrders_ShouldNeverOversellStock()
    {
        EnsureConnection();
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        var productId = await CreateProductAsync(connection, 5);
        var personId = await GetCustomerIdAsync(connection);
        var tasks = Enumerable.Range(0, 8).Select(_ => CreateOrderAsync(productId, personId, Guid.NewGuid().ToString("N"))).ToArray();
        var results = await Task.WhenAll(tasks);
        Assert.Equal(5, results.Count(x => x.Success));
        Assert.Equal(3, results.Count(x => !x.Success));
        await using var check = new SqlCommand("SELECT RemainingQuantity FROM dbo.Products WHERE Product_ID = @ProductId", connection);
        check.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
        Assert.Equal(0, Convert.ToInt32(await check.ExecuteScalarAsync()));
    }

    [SqlServerFact]
    public async Task IdempotencyKey_ShouldReplaySameRequest_AndRejectDifferentPayloadHash()
    {
        EnsureConnection();
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        var productId = await CreateProductAsync(connection, 10);
        var personId = await GetCustomerIdAsync(connection);
        var key = Guid.NewGuid().ToString("N");
        var hashA = new string('A', 64);
        var hashB = new string('B', 64);

        var first = await CreateOrderAsync(productId, personId, key, hashA);
        var replay = await CreateOrderAsync(productId, personId, key, hashA);

        Assert.True(first.Success);
        Assert.True(replay.Success);
        Assert.Equal(first.OrderId, replay.OrderId);

        var conflict = await Assert.ThrowsAsync<SqlException>(() =>
            CreateOrderAndThrowAsync(productId, personId, key, hashB));
        Assert.Equal(52012, conflict.Number);
    }

    [SqlServerFact]
    public async Task ShippedOrder_ShouldRejectCancellation()
    {
        EnsureConnection();
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        var productId = await CreateProductAsync(connection, 2);
        var personId = await GetCustomerIdAsync(connection);
        var order = await CreateOrderAsync(productId, personId, Guid.NewGuid().ToString("N"));
        Assert.True(order.Success);
        await TransitionAsync(connection, order.OrderId, "Confirmed");
        await TransitionAsync(connection, order.OrderId, "Shipped");
        var exception = await Assert.ThrowsAsync<SqlException>(() => CancelAsync(connection, order.OrderId));
        Assert.Contains("52004", exception.ToString());
    }

    private static void EnsureConnection()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw SkipException.ForSkip("Set SHOP_SQL_CONNECTION to run SQL Server integration tests.");
    }

    private static async Task<int> GetCustomerIdAsync(SqlConnection connection)
    {
        await using var command = new SqlCommand("SELECT TOP (1) Person_ID FROM dbo.Persons WHERE Role = N'Customer' AND IsActive = 1", connection);
        var value = await command.ExecuteScalarAsync();
        Assert.NotNull(value);
        return Convert.ToInt32(value);
    }

    private static async Task<int> CreateProductAsync(SqlConnection connection, int quantity)
    {
        var name = "Integration Product " + Guid.NewGuid().ToString("N");
        await using var command = new SqlCommand("INSERT INTO dbo.Products(ProductName, Category_ID, Description, UnitPrice, RemainingQuantity, IsActive) OUTPUT INSERTED.Product_ID SELECT TOP (1) @Name, Category_ID, N'Integration test product', 1.00, @Quantity, 1 FROM dbo.Categories ORDER BY Category_ID", connection);
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = name;
        command.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task<(bool Success, int OrderId)> CreateOrderAsync(int productId, int personId, string key, string? requestHash = null)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        var table = new DataTable();
        table.Columns.Add("Product_ID", typeof(int));
        table.Columns.Add("Quantity", typeof(int));
        table.Columns.Add("UnitPrice", typeof(decimal));
        table.Rows.Add(productId, 1, DBNull.Value);
        await using var command = new SqlCommand("dbo.AddListOfOrderItemToOrder", connection) { CommandType = CommandType.StoredProcedure };
        var tableParameter = command.Parameters.AddWithValue("@T", table);
        tableParameter.SqlDbType = SqlDbType.Structured;
        tableParameter.TypeName = "dbo.OrderTableType";
        command.Parameters.Add("@Person_ID", SqlDbType.Int).Value = personId;
        command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = DBNull.Value;
        command.Parameters.Add("@IdempotencyKey", SqlDbType.NVarChar, 100).Value = key;
        if (requestHash is not null)
            command.Parameters.Add("@RequestHash", SqlDbType.Char, 64).Value = requestHash;
        try
        {
            return (true, Convert.ToInt32(await command.ExecuteScalarAsync()));
        }
        catch (SqlException)
        {
            return (false, 0);
        }
    }

    private static async Task<int> CreateOrderAndThrowAsync(int productId, int personId, string key, string requestHash)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        var table = new DataTable();
        table.Columns.Add("Product_ID", typeof(int));
        table.Columns.Add("Quantity", typeof(int));
        table.Columns.Add("UnitPrice", typeof(decimal));
        table.Rows.Add(productId, 1, DBNull.Value);

        await using var command = new SqlCommand("dbo.AddListOfOrderItemToOrder", connection) { CommandType = CommandType.StoredProcedure };
        var tableParameter = command.Parameters.AddWithValue("@T", table);
        tableParameter.SqlDbType = SqlDbType.Structured;
        tableParameter.TypeName = "dbo.OrderTableType";
        command.Parameters.Add("@Person_ID", SqlDbType.Int).Value = personId;
        command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = DBNull.Value;
        command.Parameters.Add("@IdempotencyKey", SqlDbType.NVarChar, 100).Value = key;
        command.Parameters.Add("@RequestHash", SqlDbType.Char, 64).Value = requestHash;

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task TransitionAsync(SqlConnection connection, int orderId, string status)
    {
        await using var command = new SqlCommand("dbo.SP_UpdateOrders", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.Add("@Order_Id", SqlDbType.Int).Value = orderId;
        command.Parameters.Add("@D_At", SqlDbType.DateTime2).Value = DBNull.Value;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
        var rows = command.Parameters.Add("@RowsAffected", SqlDbType.Int);
        rows.Direction = ParameterDirection.Output;
        await command.ExecuteNonQueryAsync();
        Assert.Equal(1, Convert.ToInt32(rows.Value));
    }

    private static async Task CancelAsync(SqlConnection connection, int orderId)
    {
        await using var command = new SqlCommand("dbo.SP_CancelOrder", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.Add("@Order_ID", SqlDbType.Int).Value = orderId;
        var rows = command.Parameters.Add("@RowsAffected", SqlDbType.Int);
        rows.Direction = ParameterDirection.Output;
        await command.ExecuteNonQueryAsync();
    }
}
