using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.BatchValidation;

public class BatchContainsMeasurementsRule : IBatchValidationRule
{
    public bool IsFatal => true;

    public async Task<ValidationError?> ValidateAsync(Batch input)
    {
        if (input.Measurements.Count() == 0)
        {
            var error = new ValidationError(
                ValidationCode.NoMeasurements,
                $"The batch {input.BatchId} contains no measurements."
            );
            return error;
        }

        return null;
    }
}
