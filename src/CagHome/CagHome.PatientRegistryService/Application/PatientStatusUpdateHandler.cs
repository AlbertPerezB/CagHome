using CagHome.Contracts;
using CagHome.PatientRegistryService.Domain;
using CagHome.PatientRegistryService.Infrastructure;
using Wolverine;

namespace CagHome.PatientRegistryService.Application
{
    public class PatientStatusUpdateHandler
    {
        public async Task Handle(
            PatientStatusUpdateRequested message,
            IPatientRegistryStore auditStore,
            IMessageBus messageBus,
            ILogger<PatientStatusUpdateRequested> logger
        )
        {
            logger.LogDebug(
                "Patient registration update received for PatientId: {PatientId}",
                message.PatientId
            );
            var entry = new PatientRegistryEntry
            {
                PatientId = message.PatientId,
                Status = message.PatientStatus,
                LastUpdatedUtc = message.UpdatedAtUtc,
            };

            var result = await auditStore.UpdatePatientData(entry);

            if (result.IsAcknowledged)
            {
                if (result.ModifiedCount > 0 || result.UpsertedId != null)
                {
                    logger.LogDebug(
                        "Patient data updated successfully for PatientId: {PatientId}",
                        message.PatientId
                    );
                    var newMessage = new PatientStatusUpdated(
                        PatientId: message.PatientId,
                        PatientStatus: message.PatientStatus,
                        UpdatedAtUtc: message.UpdatedAtUtc
                    );

                    await messageBus.PublishAsync(newMessage);
                }
                else
                {
                    logger.LogDebug(
                        "No changes made to patient data for PatientId: {PatientId}",
                        message.PatientId
                    );
                }
            }
        }
    }
}
