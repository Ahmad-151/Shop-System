using Microsoft.Data.SqlClient;

namespace ShopAPI.Infrastructure;

/// <summary>
/// Single place that knows the connection string. Repositories ask it for a
/// (not yet opened) connection instead of each one reading configuration.
/// </summary>
public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ShopDbConnection");

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:ShopDbConnection is missing. " +
                "Set it with: dotnet user-secrets set \"ConnectionStrings:ShopDbConnection\" \"<value>\"");
        }
    }

    public SqlConnection CreateConnection() => new(_connectionString);
}
