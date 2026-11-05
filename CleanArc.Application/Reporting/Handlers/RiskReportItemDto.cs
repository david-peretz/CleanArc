namespace CleanArc.Application.Reporting.Handlers;

public sealed record RiskReportItemDto(
    Guid SagaId,
    int Age,
    int Claims,
    decimal Amount,
    string Status,
    double? Score,
    string? Decision,
    string? Reason,
    DateTime UpdatedAtUtc);
