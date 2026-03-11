using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.BatchValidation;

public class BatchValidator : Validator<Batch>
{
    public BatchValidator(PatientActiveRule patientActiveRule)
        : base(new List<IValidationRule<Batch>> { patientActiveRule }) { }

    public Task<(bool fatal, List<ValidationError> errors)> ValidateAsync(Batch batch)
    {
        return ValidateSequential(batch);
    }
}
