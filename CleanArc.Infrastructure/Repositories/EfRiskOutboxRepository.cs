using System.Text.Json;
using CleanArc.Application.Abstractions;
using CleanArc.Application.Integration;
using CleanArc.Domain.Integration;
using CleanArc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArc.Infrastructure.Repositories;

public sealed class EfRiskOutboxRepository : IRiskOutboxRepository
{
    private readonly RiskDecisionSystemDbContext _dbContext;

    public EfRiskOutboxRepository(RiskDecisionSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RiskOutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.RiskOutboxMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RiskOutboxMessage>> GetUnpublishedBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RiskOutboxMessages
            .Where(x => x.PublishedAtUtc == null)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkPublishedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var row = await _dbContext.RiskOutboxMessages.FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);
        if (row is null)
        {
            return;
        }

        row.MarkPublished();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
