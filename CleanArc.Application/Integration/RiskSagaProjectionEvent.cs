namespace CleanArc.Application.Integration;

public sealed record RiskSagaProjectionEvent(
    Guid MessageId,
    Guid SagaId,
    int Age,
    int Claims,
    decimal Amount,
    string Status,
    double? Score,
    string? Decision,
    string? Reason,
    DateTime OccurredAtUtc);
