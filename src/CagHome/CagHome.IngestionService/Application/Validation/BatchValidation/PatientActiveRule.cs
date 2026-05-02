using CagHome.Contracts.Enums;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Cache;

namespace CagHome.IngestionService.Application.Validation.BatchValidation;

public class PatientActiveRule(IPatientRegistryCache patientRegistryCache) : IBatchValidationRule
{
    public bool IsFatal => true;

    public async Task<ValidationError?> ValidateAsync(Batch input)
    {
        var status = await patientRegistryCache.GetPatientStatus(input.PatientId);
        if (status == PatientStatus.Inactive)
        {
            var error = new ValidationError(
                ValidationCode.PatientInactive,
                $"Patient {input.PatientId} is not active."
            );
            return error;
        }
        else if (status == null)
        {
            var error = new ValidationError(
                ValidationCode.PatientNotEnrolled,
                $"Patient {input.PatientId} not found in the registry."
            );
            return error;
        }

        return null;
    }
}
