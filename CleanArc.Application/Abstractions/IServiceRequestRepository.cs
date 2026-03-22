using CleanArc.Domain.Entities;

namespace CleanArc.Application.Abstractions;

public interface IServiceRequestRepository
{
    Task AddAsync(ServiceRequest request, CancellationToken cancellationToken = default);
    Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceRequest request, CancellationToken cancellationToken = default);
}
