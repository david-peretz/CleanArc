using CleanArc.Application.Integration;

namespace CleanArc.Application.Abstractions;

public interface IRiskProjectionInboxRepository
{
    Task<bool> ApplyProjectionEventAsync(RiskSagaProjectionEvent projectionEvent, CancellationToken cancellationToken = default);
}
