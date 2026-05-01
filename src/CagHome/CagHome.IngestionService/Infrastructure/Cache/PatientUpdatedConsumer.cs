using CagHome.Contracts;

namespace CagHome.IngestionService.Infrastructure.Cache;

public class PatientStatusUpdatedConsumer()
{
    public static async Task Handle(
        PatientStatusUpdated message,
        IPatientRegistryCache cache,
        ILogger<PatientStatusUpdatedConsumer> logger
    )
    {
        logger.LogInformation(
            "Patient status updated message received, updating cache for PatientId: {PatientId}",
            message.PatientId
        );
        await cache.SetPatientStatus(message.PatientId, message.PatientStatus);
    }
}
