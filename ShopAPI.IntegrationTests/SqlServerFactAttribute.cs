namespace ShopAPI.IntegrationTests;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SHOP_SQL_CONNECTION")))
            Skip = "Set SHOP_SQL_CONNECTION to run SQL Server integration tests.";
    }
}
