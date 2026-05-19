using CagHome.Contracts;

namespace CagHome.IngestionService.Infrastructure.Cache;

/// <summary>
/// Handles invoked by Wolverine when an <see cref="AllPatientStatuses"/> message is received.
/// It populates the <see cref="IPatientRegistryCache"/> with the patient statuses and marks the warmup as complete.
/// </summary>
public class AllPatientStatusesHandler
{
    public async Task Handle(
        AllPatientStatuses response,
        IPatientRegistryCache cache,
        PatientCacheWarmupService warmup,
        ILogger<AllPatientStatusesHandler> logger
    )
    {
        foreach (var patient in response.Patients)
        {
            await cache.SetPatientStatus(patient.PatientId, patient.Status);
        }

        logger.LogInformation("Loaded {Count} patients into cache", response.Patients.Count);
        warmup.Complete();
    }
}
