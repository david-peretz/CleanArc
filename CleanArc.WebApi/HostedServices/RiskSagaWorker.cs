using CleanArc.Application.Abstractions;
using CleanArc.Application.Orchestration.Commands;
using CleanArc.Application.Orchestration.Handlers;

namespace CleanArc.WebApi.HostedServices;

public sealed class RiskSagaWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RiskSagaWorker> _logger;

    public RiskSagaWorker(IServiceScopeFactory scopeFactory, ILogger<RiskSagaWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRiskAnalysisSagaRepository>();
                var processor = scope.ServiceProvider.GetRequiredService<ProcessRiskAnalysisSagaHandler>();

                var next = await repository.GetNextQueuedAsync(stoppingToken);
                if (next is null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(350), stoppingToken);
                    continue;
                }

                await processor.HandleAsync(new ProcessRiskAnalysisSagaCommand(next.Id), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Risk saga worker loop failed.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
