using CagHome.IngestionService.Application.Validation.StructuralValidation;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class StructuralValidationHandler(
    StructuralValidator validator,
    ILogger<StructuralValidationHandler> logger
) : IngestionHandler
{
    protected override async Task ProcessAsync(IngestionContext context)
    {
        logger.LogInformation("Starting structural validation");
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
