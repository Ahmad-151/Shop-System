namespace ShopAPI.Observability;

public sealed class ChannelAuditService : IAuditService
{
    private readonly AuditQueue _queue;

    public ChannelAuditService(AuditQueue queue) => _queue = queue;

    public Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(entry);
        return Task.CompletedTask;
    }
}
