namespace CleanArc.Domain.Integration;

public sealed class RiskOutboxMessage
{
    private RiskOutboxMessage()
    {
        EventType = string.Empty;
        Payload = string.Empty;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; }
    public string Payload { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }

    public static RiskOutboxMessage Create(string eventType, string payload)
    {
        return new RiskOutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            OccurredAtUtc = DateTime.UtcNow,
            PublishedAtUtc = null
        };
    }

    public void MarkPublished()
    {
        if (PublishedAtUtc is null)
        {
            PublishedAtUtc = DateTime.UtcNow;
        }
    }
}
