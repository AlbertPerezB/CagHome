using CagHome.IngestionService.Application.Validation.BatchValidation;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class BatchValidationHandler : IngestionHandler
{
    private readonly BatchValidator _validator;

    public BatchValidationHandler(BatchValidator validator)
    {
        _validator = validator;
    }

    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch == null)
            return;

        var batch = await _validator.ValidateAsync(context.Batch);
        var fatalError = batch.FatalError;
        if (fatalError != null)
        {
            context.FatalError = fatalError;
        }
        return;
    }
}
