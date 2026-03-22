using CleanArc.Application.Abstractions;
using CleanArc.Domain.Entities;
using CleanArc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArc.Infrastructure.Repositories;

public sealed class EfServiceRequestRepository : IServiceRequestRepository
{
    private readonly MunicipalDbContext _dbContext;

    public EfServiceRequestRepository(MunicipalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ServiceRequest request, CancellationToken cancellationToken = default)
    {
        await _dbContext.ServiceRequests.AddAsync(request, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .OrderByDescending(x => x.PriorityScore)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(ServiceRequest request, CancellationToken cancellationToken = default)
    {
        _dbContext.ServiceRequests.Update(request);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
