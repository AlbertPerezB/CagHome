using CagHome.Contracts;
using CagHome.PatientRegistryService.Infrastructure;

namespace CagHome.PatientRegistryService.Application;

public class AllPatientStatusesRequestedHandler
{
    public async Task<AllPatientStatuses> Handle(
        AllPatientStatusesRequested request,
        IPatientRegistryStore store,
        ILogger<AllPatientStatusesRequestedHandler> logger
    )
    {
        logger.LogDebug("Received request for all patient statuses (cache warm-up)");

        var entries = await store.GetAllPatients();

        var patients = entries.Select(e => new PatientStatusEntry(e.PatientId, e.Status)).ToList();

        logger.LogDebug("Returning {Count} patient statuses", patients.Count);

        return new AllPatientStatuses(patients);
    }
}
