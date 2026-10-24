using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ShopAPI.Infrastructure;
namespace ShopAPI.Observability;

public sealed class ShopDbHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _db;

    public ShopDbHealthCheck(IDbConnectionFactory db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _db.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy("ShopDB is reachable.");
        }
        catch (Exception exception) when (exception is SqlException or InvalidOperationException or OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("ShopDB is not reachable.", exception);
        }
    }
}
