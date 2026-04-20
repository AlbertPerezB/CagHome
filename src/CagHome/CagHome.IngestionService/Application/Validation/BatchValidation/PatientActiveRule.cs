using CagHome.Contracts.Enums;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.BatchValidation;

public class PatientActiveRule : IBatchValidationRule
{
    public bool IsFatal => true;

    public async Task<ValidationError?> ValidateAsync(Batch input)
    {
        var status = await GetPatientStatusAsync(input.PatientId);
        if (status == PatientStatus.Inactive)
        {
            var error = new ValidationError(
                ValidationCode.PatientInactive,
                $"Patient {input.PatientId} is not active."
            );
            return error;
        }

        return null;
    }

    private async Task<PatientStatus> GetPatientStatusAsync(Guid patientId)
    {
        // Simulate an asynchronous call to retrieve patient status
        await Task.Delay(100); // Simulate latency

        // For demonstration purposes, we'll assume all patients are active
        return PatientStatus.Active;
    }
}
