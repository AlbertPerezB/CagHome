using CagHome.IngestionService.Application.Validation.MeasurementValidation;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class MeasurementValidationHandler : IngestionHandler
{
    private readonly MeasurementValidator _validator;

    public MeasurementValidationHandler(MeasurementValidator validator)
    {
        _validator = validator;
    }

    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch != null)
        {
            Parallel.ForEach(
                context.Batch.Measurements,
                async measurement =>
                {
                    await _validator.ValidateAsync(measurement);
                }
            );
        }
    }
}
