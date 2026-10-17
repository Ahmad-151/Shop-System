using Microsoft.Data.SqlClient;
using ShopAPI.Infrastructure;
using System.Data;

namespace ShopAPI.Observability;

public sealed class SqlAuditStore : IAuditStore
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<SqlAuditStore> _logger;

    public SqlAuditStore(IDbConnectionFactory db, ILogger<SqlAuditStore> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.AuditEvents
            (OccurredAt, ActorId, ActorRole, Action, ResourceType, ResourceId, Outcome,
             HttpMethod, Path, StatusCode, IpAddress, CorrelationId, Details)
            VALUES
            (SYSUTCDATETIME(), @ActorId, @ActorRole, @Action, @ResourceType, @ResourceId, @Outcome,
             @HttpMethod, @Path, @StatusCode, @IpAddress, @CorrelationId, @Details);
            """;

        try
        {
            await using var connection = _db.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@ActorId", SqlDbType.Int).Value = (object?)entry.ActorId ?? DBNull.Value;
            command.Parameters.Add("@ActorRole", SqlDbType.NVarChar, 50).Value = (object?)Truncate(entry.ActorRole, 50) ?? DBNull.Value;
            command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = Truncate(entry.Action, 100)!;
            command.Parameters.Add("@ResourceType", SqlDbType.NVarChar, 100).Value = Truncate(entry.ResourceType, 100)!;
            command.Parameters.Add("@ResourceId", SqlDbType.NVarChar, 100).Value = (object?)Truncate(entry.ResourceId, 100) ?? DBNull.Value;
            command.Parameters.Add("@Outcome", SqlDbType.NVarChar, 30).Value = Truncate(entry.Outcome, 30)!;
            command.Parameters.Add("@HttpMethod", SqlDbType.NVarChar, 10).Value = Truncate(entry.HttpMethod, 10)!;
            command.Parameters.Add("@Path", SqlDbType.NVarChar, 500).Value = Truncate(entry.Path, 500)!;
            command.Parameters.Add("@StatusCode", SqlDbType.Int).Value = entry.StatusCode;
            command.Parameters.Add("@IpAddress", SqlDbType.NVarChar, 64).Value = (object?)Truncate(entry.IpAddress, 64) ?? DBNull.Value;
            command.Parameters.Add("@CorrelationId", SqlDbType.NVarChar, 64).Value = Truncate(entry.CorrelationId, 64)!;
            command.Parameters.Add("@Details", SqlDbType.NVarChar, -1).Value = (object?)entry.Details ?? DBNull.Value;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Audit event could not be persisted for {Action} {Path}", entry.Action, entry.Path);
        }
    }

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];
}
