namespace CleanArc.Application.Abstractions;

public interface IIntegrationEventQueue
{
    ValueTask PublishAsync(string eventType, string payload, CancellationToken cancellationToken = default);
    ValueTask<(string EventType, string Payload)> DequeueAsync(CancellationToken cancellationToken = default);
}
