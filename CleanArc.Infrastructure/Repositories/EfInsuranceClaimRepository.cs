using CleanArc.Application.Abstractions;
using CleanArc.Domain.Claims;
using CleanArc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArc.Infrastructure.Repositories;

public sealed class EfInsuranceClaimRepository : IInsuranceClaimRepository
{
    private readonly RiskDecisionSystemDbContext _dbContext;

    public EfInsuranceClaimRepository(RiskDecisionSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(InsuranceClaim request, CancellationToken cancellationToken = default)
    {
        await _dbContext.InsuranceClaims.AddAsync(request, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<InsuranceClaim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.InsuranceClaims.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<InsuranceClaim>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.InsuranceClaims
            .OrderByDescending(x => x.PriorityScore)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(InsuranceClaim request, CancellationToken cancellationToken = default)
    {
        _dbContext.InsuranceClaims.Update(request);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

