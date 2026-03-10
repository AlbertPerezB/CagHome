using CagHome.IngestionService.Application.Validation.Rules;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation;

public class MeasurementValidator : Validator<Measurement>
{
    protected override IEnumerable<IValidationRule<Measurement>> Rules =>
        new IValidationRule<Measurement>[]
        {
            new DeviceReportedNotInFutureRule(),
            // Add more measurement-specific validation rules here
        };
}
