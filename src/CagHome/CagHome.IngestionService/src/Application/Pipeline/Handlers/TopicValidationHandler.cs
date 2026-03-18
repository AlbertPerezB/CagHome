using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class TopicValidationHandler : IngestionHandler
{
    public TopicValidationHandler(ILoggerFactory loggerFactory)
        : base(loggerFactory) { }

    protected override Task ProcessAsync(IngestionContext context)
    {
        _logger.LogInformation("Starting topic validation");
        var topic = context.RawBatch.Topic;
        var batch = context.Batch;
        if (!string.IsNullOrWhiteSpace(topic))
        {
            if (GetIdFromTopic(topic) != batch!.PatientId)
            {
                context.FatalError = new ValidationError(
                    ValidationCode.InvalidTopic,
                    $"Patient id from batch {batch.PatientId} and topic {topic} do not match"
                );
            }
            return Task.CompletedTask;
        }

        context.FatalError = new ValidationError(
            ValidationCode.MissingRequiredField,
            $"Topic is null"
        );
        return Task.CompletedTask;
    }

    private static Guid? GetIdFromTopic(string topic)
    {
        // expects format: "biometrics/{guid}/telemetry"
        var parts = topic.Split('/');
        if (parts.Length != 3 || parts[0] != "biometrics" || parts[2] != "telemetry")
            return null;

        return Guid.TryParse(parts[1], out var id) ? id : null;
    }
}
