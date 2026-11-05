using System.Threading.Channels;
using CleanArc.Application.Abstractions;

namespace CleanArc.Infrastructure.Messaging;

public sealed class InMemoryIntegrationEventQueue : IIntegrationEventQueue
{
    private readonly Channel<(string EventType, string Payload)> _channel =
        Channel.CreateUnbounded<(string EventType, string Payload)>();

    public ValueTask PublishAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync((eventType, payload), cancellationToken);
    }

    public ValueTask<(string EventType, string Payload)> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
