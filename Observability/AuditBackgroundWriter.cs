namespace ShopAPI.Observability;

public sealed class AuditBackgroundWriter : BackgroundService
{
    private static readonly TimeSpan WriteTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DrainTimeout = TimeSpan.FromSeconds(10);

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
                using var cts = new CancellationTokenSource(WriteTimeout);
                await TryWriteAsync(entry, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }

        await DrainAsync();
    }

    private async Task DrainAsync()
    {
        _queue.Complete(); 

        using var cts = new CancellationTokenSource(DrainTimeout);
        var drained = 0;

        while (!cts.IsCancellationRequested && _queue.Reader.TryRead(out var pending))
        {
            if (await TryWriteAsync(pending, cts.Token))
                drained++;
        }

        var lost = _queue.Reader.Count;

        _logger.LogInformation(
            "Audit writer stopped. Flushed: {Drained}, left in queue: {Lost}, dropped while full: {Dropped}",
            drained, lost, _queue.DroppedCount);
    }

    private async Task<bool> TryWriteAsync(AuditEntry entry, CancellationToken ct)
    {
        try
        {
            await _store.WriteAsync(entry, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit entry");
            return false;
        }
    }
}