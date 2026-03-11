using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.MeasurementValidation;

public class DeviceReportedNotInFutureRule : IValidationRule<Measurement>
{
    public bool IsFatal => false;

    public async Task<ValidationError?> ValidateAsync(Measurement input)
    {
        if (input.DeviceReported > DateTime.UtcNow)
        {
            var error = new ValidationError(
                $"Measurement device reported time {input.DeviceReported} is in the future.",
                ValidationCode.DeviceReportedInFuture
            );

            return error;
        }

        return null;
    }
}
