using CagHome.IngestionService.Application.Validation.StructuralValidation;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class StructuralValidationHandler : IngestionHandler
{
    private readonly StructuralValidator _validator;

    public StructuralValidationHandler(StructuralValidator validator, ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _validator = validator;
    }

    protected override async Task ProcessAsync(IngestionContext context)
    {
        _logger.LogInformation("Starting structural validation");
        if (context.Json != null)
        {
            var error = await _validator.ValidateAsync(context.Json);
            if (error != null)
            {
                context.FatalError = error;
            }

            return;
        }
    }
}
