using System.Collections.Concurrent;
using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.MeasurementValidation;

public class MeasurementValidator : Validator<Measurement>
{
    public MeasurementValidator(
        CorrectUnitRule correctUnitRule,
        DeviceReportedNotInFutureRule deviceReportedNotInFutureRule
    )
        : base(
            new List<IValidationRule<Measurement>>
            {
                correctUnitRule,
                deviceReportedNotInFutureRule,
            }
        ) { }

    public Task<List<ValidationError>> ValidateAsync(IEnumerable<Measurement> measurements)
    {
        return ValidateParallel(measurements);
    }
}
