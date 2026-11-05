using CleanArc.Application.Reporting.Queries;
using CleanArc.Application.Abstractions;

namespace CleanArc.Application.Reporting.Handlers;

public sealed class GetLargeRiskReportQueryHandler
{
    private readonly IRiskReportReadRepository _repository;

    public GetLargeRiskReportQueryHandler(IRiskReportReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<LargeRiskReportResultDto> HandleAsync(GetLargeRiskReportQuery query, CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Clamp(query.PageSize, 10, 1000);
        var rows = await _repository.QueryLargeAsync(pageSize, query.UpdatedAfterUtc, query.CursorAfterId, cancellationToken);

        var mapped = rows
            .Select(x => new RiskReportItemDto(
                x.SagaId,
                x.Age,
                x.Claims,
                x.Amount,
                x.Status,
                x.Score,
                x.Decision,
                x.Reason,
                x.UpdatedAtUtc))
            .ToList();

        Guid? nextCursor = null;
        if (rows.Count == pageSize)
        {
            nextCursor = rows[^1].Id;
        }

        return new LargeRiskReportResultDto(mapped, nextCursor);
    }
}
