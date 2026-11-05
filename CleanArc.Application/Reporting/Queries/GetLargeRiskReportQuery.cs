namespace CleanArc.Application.Reporting.Queries;

public sealed record GetLargeRiskReportQuery(int PageSize = 200, DateTime? UpdatedAfterUtc = null, Guid? CursorAfterId = null);
