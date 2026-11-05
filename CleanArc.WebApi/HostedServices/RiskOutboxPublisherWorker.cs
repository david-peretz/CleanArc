using CleanArc.Application.Abstractions;

namespace CleanArc.WebApi.HostedServices;

public sealed class RiskOutboxPublisherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RiskOutboxPublisherWorker> _logger;

    public RiskOutboxPublisherWorker(IServiceScopeFactory scopeFactory, ILogger<RiskOutboxPublisherWorker> logger)
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
                var outbox = scope.ServiceProvider.GetRequiredService<IRiskOutboxRepository>();
                var queue = scope.ServiceProvider.GetRequiredService<IIntegrationEventQueue>();

                var batch = await outbox.GetUnpublishedBatchAsync(100, stoppingToken);
                if (batch.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(300), stoppingToken);
                    continue;
                }

                foreach (var message in batch)
                {
                    await queue.PublishAsync(message.EventType, message.Payload, stoppingToken);
                    await outbox.MarkPublishedAsync(message.Id, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publisher failed.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
