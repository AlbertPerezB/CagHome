using Microsoft.Extensions.Logging;
using Wolverine;

namespace CagHome.IngestionService.Infrastructure.Messaging;

public sealed class RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, IMessageBus messageBus)
{
    public async Task PublishPingAsync(PingMessage message, CancellationToken cancellationToken)
    {
        // Implementation of RabbitMQ publisher
        logger.LogInformation(
            "Publishing ping: CorrelationId={CorrelationId}, Sequence={Sequence}/{MaxTurns}",
            message.CorrelationId,
            message.Sequence,
            message.MaxTurns
        );

        await messageBus.PublishAsync(message);
    }

    public async Task PublishPongAsync(PongMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Publishing pong: CorrelationId={CorrelationId}, Sequence={Sequence}/{MaxTurns}",
            message.CorrelationId,
            message.Sequence,
            message.MaxTurns
        );

        await messageBus.PublishAsync(message);
    }
}
