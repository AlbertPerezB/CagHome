using CagHome.Contracts.Enums;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Cache;

namespace CagHome.IngestionService.Application.Validation.BatchValidation;

/// <summary>
/// Validation rule to check if the patient associated with the batch is active in the patient registry cache.
/// </summary>
/// <param name="patientRegistryCache">The patient registry cache to check the patient's status.</param>
public class PatientActiveRule(IPatientRegistryCache patientRegistryCache) : IBatchValidationRule
{
    public bool IsFatal => true;

    public async Task<ValidationError?> ValidateAsync(Batch input)
    {
        var status = await patientRegistryCache.GetPatientStatus(input.PatientId);
        if (status == PatientStatus.Inactive || status == PatientStatus.Deceased)
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
