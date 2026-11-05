using CleanArc.Application.Abstractions;
using CleanArc.Domain.Enums;
using CleanArc.Domain.Orchestration;
using CleanArc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArc.Infrastructure.Repositories;

public sealed class EfRiskAnalysisSagaRepository : IRiskAnalysisSagaRepository
{
    private readonly RiskDecisionSystemDbContext _dbContext;

    public EfRiskAnalysisSagaRepository(RiskDecisionSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RiskAnalysisSaga saga, CancellationToken cancellationToken = default)
    {
        await _dbContext.RiskAnalysisSagas.AddAsync(saga, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<RiskAnalysisSaga?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.RiskAnalysisSagas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<RiskAnalysisSaga?> GetNextQueuedAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.RiskAnalysisSagas
            .Where(x => x.Status == RiskSagaStatus.Queued)
            .OrderBy(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(RiskAnalysisSaga saga, CancellationToken cancellationToken = default)
    {
        _dbContext.RiskAnalysisSagas.Update(saga);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
