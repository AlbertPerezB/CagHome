using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

/// <summary>
/// Validates that the topic from the raw batch matches the patient id in the batch dto, and that the topic is in the expected format.
/// </summary>
public class TopicValidationHandler(ILogger<TopicValidationHandler> logger) : IngestionHandler
{
    protected override Task ProcessAsync(IngestionContext context)
    {
        logger.LogDebug("Starting topic validation");
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

    /// <summary>
    /// Extracts the patient id from the topic. Expects the topic to be in the format "biometrics/{patientId}/telemetry".
    /// </summary>
    /// <param name="topic"> The topic string </param>
    /// <returns> Returns null if the patient id is not in the expected format, or the patient id is not a valid guid. </returns>
    private static Guid? GetIdFromTopic(string topic)
    {
        var parts = topic.Split('/');
        if (parts.Length != 3 || parts[0] != "biometrics" || parts[2] != "telemetry")
            return null;

        return Guid.TryParse(parts[1], out var id) ? id : null;
    }
}
