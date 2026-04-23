using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RabbitMQBroker;

public sealed class RabbitMqBrokerWorker(ILogger<RabbitMqBrokerWorker> logger, IConnection connection) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("RabbitMQ connection status: {Status}", connection.IsOpen ? "Open" : "Closed");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
