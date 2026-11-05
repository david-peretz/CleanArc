using CleanArc.Application.Abstractions;
using CleanArc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArc.Infrastructure.Repositories;

public sealed class EfRiskReportReadRepository : IRiskReportReadRepository
{
    private readonly ReportingDbContext _dbContext;

    public EfRiskReportReadRepository(ReportingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RiskReportReadRow>> QueryLargeAsync(
        int pageSize,
        DateTime? updatedAfterUtc,
        Guid? cursorAfterId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.RiskReportProjections.AsNoTracking().AsQueryable();

        if (updatedAfterUtc.HasValue)
        {
            query = query.Where(x => x.UpdatedAtUtc >= updatedAfterUtc.Value);
        }

        if (cursorAfterId.HasValue)
        {
            query = query.Where(x => x.Id.CompareTo(cursorAfterId.Value) > 0);
        }

        return await query
            .OrderBy(x => x.Id)
            .Take(pageSize)
            .Select(x => new RiskReportReadRow(
                x.Id,
                x.SagaId,
                x.Age,
                x.Claims,
                x.Amount,
                x.Status,
                x.Score,
                x.Decision,
                x.Reason,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
