using CleanArc.Application.Abstractions;
using CleanArc.Application.Integration;
using CleanArc.Domain.Reporting;
using CleanArc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArc.Infrastructure.Repositories;

public sealed class EfRiskProjectionInboxRepository : IRiskProjectionInboxRepository
{
    private readonly ReportingDbContext _dbContext;

    public EfRiskProjectionInboxRepository(ReportingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ApplyProjectionEventAsync(RiskSagaProjectionEvent projectionEvent, CancellationToken cancellationToken = default)
    {
        var alreadyProcessed = await _dbContext.RiskInboxMessages
            .AnyAsync(x => x.Id == projectionEvent.MessageId, cancellationToken);

        if (alreadyProcessed)
        {
            return false;
        }

        var projection = await _dbContext.RiskReportProjections
            .FirstOrDefaultAsync(x => x.SagaId == projectionEvent.SagaId, cancellationToken);

        if (projection is null)
        {
            projection = new RiskReportProjection
            {
                Id = Guid.NewGuid(),
                SagaId = projectionEvent.SagaId
            };
            await _dbContext.RiskReportProjections.AddAsync(projection, cancellationToken);
        }

        projection.Age = projectionEvent.Age;
        projection.Claims = projectionEvent.Claims;
        projection.Amount = projectionEvent.Amount;
        projection.Status = projectionEvent.Status;
        projection.Score = projectionEvent.Score;
        projection.Decision = projectionEvent.Decision;
        projection.Reason = projectionEvent.Reason;
        projection.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.RiskInboxMessages.AddAsync(
            RiskInboxMessage.Create(projectionEvent.MessageId, nameof(RiskSagaProjectionEvent)),
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
