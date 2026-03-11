using CagHome.IngestionService.Application.Validation.StructuralValidation;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class StructuralValidationHandler : IngestionHandler
{
    private readonly StructuralValidator _validator;

    public StructuralValidationHandler(StructuralValidator validator)
    {
        _validator = validator;
    }

    protected override async Task ProcessAsync(IngestionContext context)
    {
        var error = await _validator.ValidateAsync(context.RawBatch);
        if (error != null)
        {
            context.fatalError = error;
        }
        return;
    }
}
