using CagHome.MonitoringService.Infrastructure;

namespace CagHome.MonitoringService.Application.Handlers;

public sealed class CareplanUpdateRequestedHandler
{
    public async Task Handle(
        CareplanUpdateRequested message,
        IPatientCareplanStore patientCareplanStore,
        ILogger<CareplanUpdateRequestedHandler> logger
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
