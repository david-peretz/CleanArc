using CleanArc.Application.Abstractions;
using CleanArc.Domain.Entities;
using CleanArc.Domain.Enums;
using System.Collections.Concurrent;

namespace CleanArc.Infrastructure.Repositories;

public sealed class InMemoryServiceRequestRepository : IServiceRequestRepository
{
    private readonly ConcurrentDictionary<Guid, ServiceRequest> _store = new();

    public Task AddAsync(ServiceRequest request, CancellationToken cancellationToken = default)
    {
        _store.TryAdd(request.Id, request);
        return Task.CompletedTask;
    }

    public Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var request);
        return Task.FromResult(request);
    }

    public Task<IReadOnlyList<ServiceRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = _store.Values
            .OrderByDescending(x => x.PriorityScore)
            .ThenBy(x => x.CreatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<ServiceRequest>>(list);
    }

    public Task UpdateAsync(ServiceRequest request, CancellationToken cancellationToken = default)
    {
        _store[request.Id] = request;
        return Task.CompletedTask;
    }
}
