using CleanArc.Application.Abstractions;
using CleanArc.Application.Contracts;
using CleanArc.Application.Integration;
using CleanArc.Application.Orchestration.Commands;
using CleanArc.Application.Services;
using CleanArc.Domain.Integration;
using System.Text.Json;

namespace CleanArc.Application.Orchestration.Handlers;

public sealed class ProcessRiskAnalysisSagaHandler
{
    private readonly IRiskAnalysisSagaRepository _repository;
    private readonly IRiskOutboxRepository _outboxRepository;
    private readonly InsuranceRiskAnalysisService _riskService;

    public ProcessRiskAnalysisSagaHandler(
        IRiskAnalysisSagaRepository repository,
        IRiskOutboxRepository outboxRepository,
        InsuranceRiskAnalysisService riskService)
    {
        _repository = repository;
        _outboxRepository = outboxRepository;
        _riskService = riskService;
    }

    public async Task HandleAsync(ProcessRiskAnalysisSagaCommand command, CancellationToken cancellationToken = default)
    {
        var saga = await _repository.GetByIdAsync(command.SagaId, cancellationToken);
        if (saga is null)
        {
            return;
        }

        if (saga.Status != CleanArc.Domain.Enums.RiskSagaStatus.Queued)
        {
            return;
        }

        saga.MarkRunning();
        await _repository.UpdateAsync(saga, cancellationToken);

        try
        {
            var result = await _riskService.AnalyzeAsync(
                new RiskAnalysisCommand(saga.Age, saga.Claims, saga.Amount),
                cancellationToken);

            saga.MarkCompleted(result.Score, result.Decision, result.Reason);
            await _repository.UpdateAsync(saga, cancellationToken);
            await _outboxRepository.AddAsync(CreateProjectionOutbox(saga), cancellationToken);
        }
        catch (Exception ex)
        {
            saga.MarkFailed(ex.Message);
            await _repository.UpdateAsync(saga, cancellationToken);
            await _outboxRepository.AddAsync(CreateProjectionOutbox(saga), cancellationToken);
        }
    }

    private static RiskOutboxMessage CreateProjectionOutbox(CleanArc.Domain.Orchestration.RiskAnalysisSaga saga)
    {
        var evt = new RiskSagaProjectionEvent(
            Guid.NewGuid(),
            saga.Id,
            saga.Age,
            saga.Claims,
            saga.Amount,
            saga.Status.ToString(),
            saga.Score,
            saga.Decision?.ToString(),
            string.IsNullOrWhiteSpace(saga.Reason) ? null : saga.Reason,
            DateTime.UtcNow);

        return RiskOutboxMessage.Create(
            eventType: nameof(RiskSagaProjectionEvent),
            payload: JsonSerializer.Serialize(evt));
    }
}
