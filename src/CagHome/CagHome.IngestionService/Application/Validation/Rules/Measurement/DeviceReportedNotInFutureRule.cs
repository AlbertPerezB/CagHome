using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.Rules;

public class DeviceReportedNotInFutureRule : IValidationRule<Measurement>
{
    public bool StopOnFailure => false;

    Task<ValidationResult> IValidationRule<Measurement>.ValidateAsync(
        Measurement input,
        CancellationToken ct
    )
    {
        if (input.DeviceReported > DateTime.UtcNow)
        {
            return Task.FromResult(
                ValidationResult.Failure(
                    $"Measurement device reported time {input.DeviceReported} is in the future.",
                    ValidationCode.DeviceReportedInFuture
                )
            );
        }

        return Task.FromResult(ValidationResult.Success());
    }
}
