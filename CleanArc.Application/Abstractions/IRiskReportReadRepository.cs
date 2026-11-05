namespace CleanArc.Application.Abstractions;

public sealed record RiskReportReadRow(
    Guid Id,
    Guid SagaId,
    int Age,
    int Claims,
    decimal Amount,
    string Status,
    double? Score,
    string? Decision,
    string? Reason,
    DateTime UpdatedAtUtc);

public interface IRiskReportReadRepository
{
    Task<IReadOnlyList<RiskReportReadRow>> QueryLargeAsync(
        int pageSize,
        DateTime? updatedAfterUtc,
        Guid? cursorAfterId,
        CancellationToken cancellationToken = default);
}
