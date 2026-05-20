using CagHome.IngestionService.Application.Validation.MeasurementValidation;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

/// <summary>
/// Handler responsible for validating each measurement in the batch using the provided <see cref="MeasurementValidator"/>.
/// Validation is performed in parallel for efficiency.
/// </summary>
public class MeasurementValidationHandler(
    MeasurementValidator validator,
    ILogger<MeasurementValidationHandler> logger
) : IngestionHandler
{
    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch != null)
        {
            logger.LogDebug("Starting measurement validation");
            Parallel.ForEach(
                context.Batch.Measurements,
                async measurement =>
                {
                    await validator.ValidateAsync(measurement);
                }
            );
        }
    }
}
