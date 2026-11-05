using CleanArc.Application.Abstractions;

namespace CleanArc.Application.Orchestration.Handlers;

public sealed class GetRiskAnalysisSagaHandler
{
    private readonly IRiskAnalysisSagaRepository _repository;

    public GetRiskAnalysisSagaHandler(IRiskAnalysisSagaRepository repository)
    {
        _repository = repository;
    }

    public async Task<RiskAnalysisSagaDto?> HandleAsync(Guid sagaId, CancellationToken cancellationToken = default)
    {
        var saga = await _repository.GetByIdAsync(sagaId, cancellationToken);
        if (saga is null)
        {
            return null;
        }

        return new RiskAnalysisSagaDto(
            saga.Id,
            saga.Age,
            saga.Claims,
            saga.Amount,
            saga.Status,
            saga.Score,
            saga.Decision,
            saga.Reason,
            saga.Error,
            saga.CreatedAtUtc,
            saga.UpdatedAtUtc);
    }
}
