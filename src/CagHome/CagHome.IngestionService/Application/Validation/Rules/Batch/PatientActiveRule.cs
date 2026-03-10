using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation
{
    public class PatientActiveRule : IValidationRule<Batch>
    {
        public bool StopOnFailure => true;

        public async Task<ValidationResult> ValidateAsync(
            Batch batch,
            CancellationToken ct = default
        )
        {
            //check cache
            return ValidationResult.Success();
        }
    }
}
