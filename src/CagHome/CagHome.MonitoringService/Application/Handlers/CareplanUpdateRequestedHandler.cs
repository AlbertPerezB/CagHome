using CagHome.MonitoringService.Infrastructure;

namespace CagHome.MonitoringService.Application.Handlers;

/// <summary>
/// Handles careplan update requests by updating the patient careplan state.
/// </summary>
public static class CareplanUpdateRequestedHandler
{
    /// <summary>
    /// Handles an incoming careplan update request for a patient.
    /// </summary>
    /// <param name="message">The careplan update request message.</param>
    /// <param name="patientCareplanStore">Store used to upsert patient careplan state.</param>
    /// <param name="logger">Logger used to record careplan update activity.</param>
    /// <returns>A task when the update has been completed.</returns>
    public static async Task Handle(
        CareplanUpdateRequested message,
        IPatientCareplanStore patientCareplanStore,
        ILogger<CareplanUpdateRequested> logger
    )
    {
        await patientCareplanStore.Upsert(message.PatientId, message.Careplan, message.UpdatedAtUtc);

        logger.LogInformation(
            "Careplan updated: PatientId={PatientId}, Careplan={Careplan}, UpdatedAtUtc={UpdatedAtUtc}",
            message.PatientId,
            message.Careplan,
            message.UpdatedAtUtc
        );
    }
}
