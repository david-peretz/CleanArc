namespace CleanArc.Application.Reporting.Handlers;

public sealed record LargeRiskReportResultDto(
    IReadOnlyList<RiskReportItemDto> Items,
    Guid? NextCursor);
