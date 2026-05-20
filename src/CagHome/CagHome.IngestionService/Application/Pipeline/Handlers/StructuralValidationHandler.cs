using CagHome.IngestionService.Application.Validation.StructuralValidation;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

/// <summary>
/// Handler responsible for validating the structure of the incoming JSON against the expected schema version.
/// If the JSON does not conform to the expected structure, a fatal error is set in the context.
/// </summary>
public class StructuralValidationHandler(
    StructuralValidator validator,
    ILogger<StructuralValidationHandler> logger
) : IngestionHandler
{
    protected override async Task ProcessAsync(IngestionContext context)
    {
        logger.LogDebug("Starting structural validation");
        if (context.Json != null)
        {
            var error = await validator.ValidateAsync(context.Json);
            if (error != null)
            {
                context.FatalError = error;
            }

            return;
        }
    }
}
