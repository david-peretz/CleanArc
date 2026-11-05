namespace CleanArc.Domain.Reporting;

public sealed class RiskInboxMessage
{
    private RiskInboxMessage()
    {
        EventType = string.Empty;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }
    public DateTime ProcessedAtUtc { get; private set; }

    public static RiskInboxMessage Create(Guid messageId, string eventType)
    {
        return new RiskInboxMessage
        {
            Id = messageId,
            EventType = eventType,
            ReceivedAtUtc = DateTime.UtcNow,
            ProcessedAtUtc = DateTime.UtcNow
        };
    }
}
