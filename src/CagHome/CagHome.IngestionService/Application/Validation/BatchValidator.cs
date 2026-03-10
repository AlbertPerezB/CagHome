using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation;

public class BatchValidator : Validator<Batch>
{
    protected override IEnumerable<IValidationRule<Batch>> Rules =>
        new IValidationRule<Batch>[]
        {
            new PatientActiveRule(),
            // Add more batch-specific validation rules here
        };
}
