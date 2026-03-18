namespace CagHome.IngestionService.Infrastructure.Messaging;

public sealed record PingMessage(
    Guid CorrelationId,
    int Sequence,
    int MaxTurns,
    DateTime SentAtUtc
);

public sealed record PongMessage(
    Guid CorrelationId,
    int Sequence,
    int MaxTurns,
    DateTime SentAtUtc
);

public static class PingPongTopology
{
    public const string QueueName = "ingestionservice-pingpong";
}
