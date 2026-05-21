using CagHome.Contracts;
using CagHome.PatientRegistryService.Domain;
using CagHome.PatientRegistryService.Infrastructure;
using Wolverine;

namespace CagHome.PatientRegistryService.Application
{
    /// <summary>
    /// Handles patient status update requests by updating the patient registry and publishing status update events as
    /// needed.
    /// </summary>
    /// <remarks>This handler processes incoming patient status update messages, updates the patient registry
    /// store, and publishes a message if the update results in a change.</remarks>
    public class PatientStatusUpdateHandler
    {
        /// <summary>
        /// Handles a patient status update request by updating the patient registry. If the patient data is updated
        /// or inserted, a notification message is published to the message bus. No notification is sent if no changes
        /// are made to the patient data.
        /// </summary>
        /// <param name="message">The patient status update request message containing the patient identifier, new status, and update
        /// timestamp.</param>
        /// <param name="auditStore">The patient registry store used to update patient data and audit changes.</param>
        /// <param name="messageBus">The message bus used to publish a notification when the patient status is updated.</param>
        /// <param name="logger">The logger used to record informational messages about the update process.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task Handle(
            PatientStatusUpdateRequested message,
            IPatientRegistryStore auditStore,
            IMessageBus messageBus,
            ILogger<PatientStatusUpdateRequested> logger
        )
        {
            logger.LogInformation(
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
                    logger.LogInformation(
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
                    logger.LogInformation(
                        "No changes made to patient data for PatientId: {PatientId}",
                        message.PatientId
                    );
                }
            }
        }
    }
}
