using CagHome.IngestionService.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace CagHome.IngestionService.Infrastructure.Messaging;

public sealed class RabbitMqConsumer(ILogger<RabbitMqConsumer> logger, RabbitMqPublisher publisher)
{
    public async Task Handle(PingMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Consumed ping: CorrelationId={CorrelationId}, Sequence={Sequence}/{MaxTurns}",
            message.CorrelationId,
            message.Sequence,
            message.MaxTurns
        );

        if (message.Sequence >= message.MaxTurns)
        {
            logger.LogInformation(
                "Ping/Pong completed at ping sequence {Sequence}",
                message.Sequence
            );
            return;
        }

        var next = new PongMessage(
            message.CorrelationId,
            message.Sequence + 1,
            message.MaxTurns,
            DateTime.UtcNow
        );

        await publisher.PublishPongAsync(next, cancellationToken);
    }

    public async Task Handle(PongMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Consumed pong: CorrelationId={CorrelationId}, Sequence={Sequence}/{MaxTurns}",
            message.CorrelationId,
            message.Sequence,
            message.MaxTurns
        );

        if (message.Sequence >= message.MaxTurns)
        {
            logger.LogInformation(
                "Ping/Pong completed at pong sequence {Sequence}",
                message.Sequence
            );
            return;
        }

        var next = new PingMessage(
            message.CorrelationId,
            message.Sequence + 1,
            message.MaxTurns,
            DateTime.UtcNow
        );

        await publisher.PublishPingAsync(next, cancellationToken);
    }
}
