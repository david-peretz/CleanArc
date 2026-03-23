using CleanArc.Domain.Claims;

namespace CleanArc.Application.Abstractions;

public interface IInsuranceClaimRepository
{
    Task AddAsync(InsuranceClaim request, CancellationToken cancellationToken = default);
    Task<InsuranceClaim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InsuranceClaim>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(InsuranceClaim request, CancellationToken cancellationToken = default);
}

