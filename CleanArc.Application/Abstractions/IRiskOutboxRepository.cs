using CleanArc.Domain.Integration;

namespace CleanArc.Application.Abstractions;

public interface IRiskOutboxRepository
{
    Task AddAsync(RiskOutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RiskOutboxMessage>> GetUnpublishedBatchAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkPublishedAsync(Guid messageId, CancellationToken cancellationToken = default);
}
