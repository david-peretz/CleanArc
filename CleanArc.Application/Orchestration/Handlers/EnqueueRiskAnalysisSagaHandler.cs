using CleanArc.Application.Abstractions;
using CleanArc.Application.Orchestration.Commands;
using CleanArc.Domain.Orchestration;

namespace CleanArc.Application.Orchestration.Handlers;

public sealed class EnqueueRiskAnalysisSagaHandler
{
    private readonly IRiskAnalysisSagaRepository _repository;

    public EnqueueRiskAnalysisSagaHandler(IRiskAnalysisSagaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(EnqueueRiskAnalysisSagaCommand command, CancellationToken cancellationToken = default)
    {
        var saga = RiskAnalysisSaga.Enqueue(command.Age, command.Claims, command.Amount);
        await _repository.AddAsync(saga, cancellationToken);
        return saga.Id;
    }
}
