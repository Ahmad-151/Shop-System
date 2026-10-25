using System.Threading.Channels;

namespace ShopAPI.Observability;

public sealed class AuditQueue
{
    private readonly Channel<AuditEntry> _channel;
    private long _dropped;

    public AuditQueue()
    {
        _channel = Channel.CreateBounded<AuditEntry>(
            new BoundedChannelOptions(capacity: 1000)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false
            },
            itemDropped: _ => Interlocked.Increment(ref _dropped));
    }

    public void Enqueue(AuditEntry entry) => _channel.Writer.TryWrite(entry);

    public ChannelReader<AuditEntry> Reader => _channel.Reader;

    public long DroppedCount => Interlocked.Read(ref _dropped);

    public void Complete() => _channel.Writer.TryComplete();
}