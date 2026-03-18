using CagHome.IngestionService.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CagHome.IngestionService.Infrastructure.Messaging;

// This class is just for starting the ping/pong workflow. A small test to get wolverine running.

public sealed class PingPongStartupService(
    ILogger<PingPongStartupService> logger,
    RabbitMqPublisher publisher
) : IHostedLifecycleService
{
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        try
        {
            var initial = new PingMessage(
                Guid.NewGuid(),
                Sequence: 1,
                MaxTurns: 10,
                SentAtUtc: DateTime.UtcNow
            );

            logger.LogInformation(
                "Starting ping/pong workflow: CorrelationId={CorrelationId}, MaxTurns={MaxTurns}",
                initial.CorrelationId,
                initial.MaxTurns
            );

            await publisher.PublishPingAsync(initial, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish initial ping after application startup");
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
