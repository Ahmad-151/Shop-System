using System.Threading.Channels;

namespace ShopAPI.Observability;

public sealed class AuditQueue
{
    private readonly Channel<AuditEntry> _channel;
    private long _dropped;

    public AuditQueue()
    {
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<AuditEntry>(
            options,
            _ => Interlocked.Increment(ref _dropped));
    }

    public void Enqueue(AuditEntry entry) => _channel.Writer.TryWrite(entry);

    public ChannelReader<AuditEntry> Reader => _channel.Reader;

    public long DroppedCount => Interlocked.Read(ref _dropped);

    public void Complete() => _channel.Writer.TryComplete();
}