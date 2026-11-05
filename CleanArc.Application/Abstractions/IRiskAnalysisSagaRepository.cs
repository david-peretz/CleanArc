using CleanArc.Domain.Orchestration;

namespace CleanArc.Application.Abstractions;

public interface IRiskAnalysisSagaRepository
{
    Task AddAsync(RiskAnalysisSaga saga, CancellationToken cancellationToken = default);
    Task<RiskAnalysisSaga?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RiskAnalysisSaga?> GetNextQueuedAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(RiskAnalysisSaga saga, CancellationToken cancellationToken = default);
}
