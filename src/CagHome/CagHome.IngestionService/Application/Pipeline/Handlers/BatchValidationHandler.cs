using CagHome.IngestionService.Application.Validation.BatchValidation;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class BatchValidationHandler(
    BatchValidator validator,
    ILogger<BatchValidationHandler> logger
) : IngestionHandler
{
    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch != null)
        {
            logger.LogDebug("Starting Batch validation.");
            var batch = await validator.ValidateAsync(context.Batch);
            var fatalError = batch.FatalError;
            if (fatalError != null)
            {
                context.FatalError = fatalError;
            }
        }
    }
}
