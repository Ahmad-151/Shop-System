namespace ShopAPI.Observability;
public interface IAuditService
{
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}

public interface IAuditStore
{
    Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}

public sealed record AuditEntry(
    string Action,
    string ResourceType,
    string? ResourceId,
    string Outcome,
    int StatusCode,
    int? ActorId,
    string? ActorRole,
    string HttpMethod,
    string Path,
    string? IpAddress,
    string CorrelationId,
    string? Details);
