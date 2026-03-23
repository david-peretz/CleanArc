using CleanArc.Application.Abstractions;
using CleanArc.Domain.Claims;
using CleanArc.Domain.Enums;
using System.Collections.Concurrent;

namespace CleanArc.Infrastructure.Repositories;

public sealed class InMemoryInsuranceClaimRepository : IInsuranceClaimRepository
{
    private readonly ConcurrentDictionary<Guid, InsuranceClaim> _store = new();

    public Task AddAsync(InsuranceClaim request, CancellationToken cancellationToken = default)
    {
        _store.TryAdd(request.Id, request);
        return Task.CompletedTask;
    }

    public Task<InsuranceClaim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var request);
        return Task.FromResult(request);
    }

    public Task<IReadOnlyList<InsuranceClaim>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = _store.Values
            .OrderByDescending(x => x.PriorityScore)
            .ThenBy(x => x.CreatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<InsuranceClaim>>(list);
    }

    public Task UpdateAsync(InsuranceClaim request, CancellationToken cancellationToken = default)
    {
        _store[request.Id] = request;
        return Task.CompletedTask;
    }
}

