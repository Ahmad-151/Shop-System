namespace ShopAPI.Observability;

public sealed class AuditBackgroundWriter : BackgroundService
{
    private const int MaxDrainOnShutdown = 200;

    private readonly AuditQueue _queue;
    private readonly IAuditStore _store;
    private readonly ILogger<AuditBackgroundWriter> _logger;

    public AuditBackgroundWriter(AuditQueue queue, IAuditStore store, ILogger<AuditBackgroundWriter> logger)
    {
        _queue = queue;
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var entry in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                await _store.WriteAsync(entry, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }

        var drained = 0;
        while (drained < MaxDrainOnShutdown && _queue.Reader.TryRead(out var pending))
        {
            await _store.WriteAsync(pending, CancellationToken.None);
            drained++;
        }

        _logger.LogInformation("Audit writer stopped after flushing {Count} pending events", drained);
    }
}
