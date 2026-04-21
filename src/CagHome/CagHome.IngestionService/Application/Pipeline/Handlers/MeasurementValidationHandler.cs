using CagHome.IngestionService.Application.Validation.MeasurementValidation;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class MeasurementValidationHandler(
    MeasurementValidator validator,
    ILogger<MeasurementValidationHandler> logger
) : IngestionHandler
{
    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch != null)
        {
            logger.LogInformation("Starting measurement validation");
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
