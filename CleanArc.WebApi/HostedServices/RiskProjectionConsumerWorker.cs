using System.Text.Json;
using CleanArc.Application.Abstractions;
using CleanArc.Application.Integration;

namespace CleanArc.WebApi.HostedServices;

public sealed class RiskProjectionConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RiskProjectionConsumerWorker> _logger;

    public RiskProjectionConsumerWorker(IServiceScopeFactory scopeFactory, ILogger<RiskProjectionConsumerWorker> logger)
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
                var queue = scope.ServiceProvider.GetRequiredService<IIntegrationEventQueue>();
                var projectionInbox = scope.ServiceProvider.GetRequiredService<IRiskProjectionInboxRepository>();

                var (eventType, payload) = await queue.DequeueAsync(stoppingToken);
                if (!string.Equals(eventType, nameof(RiskSagaProjectionEvent), StringComparison.Ordinal))
                {
                    continue;
                }

                var evt = JsonSerializer.Deserialize<RiskSagaProjectionEvent>(payload);
                if (evt is null)
                {
                    continue;
                }

                await projectionInbox.ApplyProjectionEventAsync(evt, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection consumer failed.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
