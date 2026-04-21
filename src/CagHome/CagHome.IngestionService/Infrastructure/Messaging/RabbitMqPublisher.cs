using CagHome.Contracts;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace CagHome.IngestionService.Infrastructure.Messaging;

public sealed class RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, IMessageBus messageBus)
    : IRabbitMqPublisher
{
    public async Task PublishBatchReceived(BatchReceived message)
    {
        logger.LogInformation(
            $"Publishing BatchReceived {message.BatchId} for patient {message.PatientId} with {message.Measurements.Count} measurements"
        );
        await messageBus.PublishAsync(message);
    }
}
