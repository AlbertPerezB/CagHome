using CagHome.Contracts;

namespace CagHome.IngestionService.Infrastructure.Cache;

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
