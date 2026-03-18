using CagHome.IngestionService.Application.Validation.MeasurementValidation;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class MeasurementValidationHandler : IngestionHandler
{
    private readonly MeasurementValidator _validator;

    public MeasurementValidationHandler(
        MeasurementValidator validator,
        ILoggerFactory loggerFactory
    )
        : base(loggerFactory)
    {
        _validator = validator;
    }

    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch != null)
        {
            _logger.LogInformation("Starting measurement validation");
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
