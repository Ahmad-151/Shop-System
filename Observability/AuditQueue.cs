using System.Threading.Channels;

namespace ShopAPI.Observability;
public sealed class AuditQueue
{
    private readonly Channel<AuditEntry> _channel = Channel.CreateBounded<AuditEntry>(
        new BoundedChannelOptions(capacity: 1000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true
        });

    public void Enqueue(AuditEntry entry) => _channel.Writer.TryWrite(entry);

    public ChannelReader<AuditEntry> Reader => _channel.Reader;
}
