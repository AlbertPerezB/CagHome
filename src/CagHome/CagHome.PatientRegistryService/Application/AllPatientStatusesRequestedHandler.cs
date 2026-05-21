using CagHome.Contracts;
using CagHome.PatientRegistryService.Infrastructure;

namespace CagHome.PatientRegistryService.Application;

/// <summary>
/// Handles requests to retrieve the statuses of all patients from the patient registry store.
/// </summary>
public class AllPatientStatusesRequestedHandler
{
    /// <summary>
    /// Handles a request to retrieve the current status of all patients from the patient registry store.
    /// </summary>
    /// <remarks>This method is typically used to warm up the cache by retrieving all patient statuses.
    /// Logging is performed at debug and information levels to trace the operation.</remarks>
    /// <param name="request">The request message containing parameters for retrieving all patient statuses.</param>
    /// <param name="store">The patient registry store used to access patient status data.</param>
    /// <param name="logger">The logger used to record diagnostic and operational information during the handling of the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an AllPatientStatuses object with
    /// the status entries for all patients.</returns>
    public async Task<AllPatientStatuses> Handle(
        AllPatientStatusesRequested request,
        IPatientRegistryStore store,
        ILogger<AllPatientStatusesRequestedHandler> logger
    )
    {
        logger.LogDebug("Received request for all patient statuses (cache warm-up)");

        var entries = await store.GetAllPatients();

        var patients = entries.Select(e => new PatientStatusEntry(e.PatientId, e.Status)).ToList();

        logger.LogInformation("Returning {Count} patient statuses (cache-warm-up)", patients.Count);

        return new AllPatientStatuses(patients);
    }
}
