using CagHome.Contracts;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;

namespace CagHome.NotificationService.Application.Handlers;

/// <summary>
/// Handles a patient alert request by publishing the alert and recording the delivery status in the audit store.
/// </summary>
public class PatientAlertHandler
{
    /// <summary>
    /// Processes a patient alert request by publishing the alert message and recording the delivery status in the audit
    /// store.
    /// </summary>
    /// <remarks>An audit entry is recorded for each delivery attempt, successful delivery, or failure. If
    /// publishing the alert fails, the exception is propagated after recording the failure.</remarks>
    /// <param name="message">The patient alert request message containing the patient identifier and alert details to be published.</param>
    /// <param name="mqttPublisher">The MQTT publisher used to send the alert message to the specified patient.</param>
    /// <param name="auditStore">The audit store used to record the delivery status of the alert message.</param>
    /// <param name="logger">The logger used for logging diagnostic information related to the patient alert request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Handle(
        PatientAlertRequested message,
        IMqttPublisher mqttPublisher,
        IAuditStore auditStore,
        ILogger<PatientAlertRequested> logger
    )
    {
        await auditStore.RecordAuditEntry(new AuditEntry(message, DeliveryStatus.Attempted));

        try
        {
            await mqttPublisher.Publish(
                message.PatientId,
                new { message.Message, Timestamp = DateTime.UtcNow }
            );

            await auditStore.RecordAuditEntry(new AuditEntry(message, DeliveryStatus.Delivered));
        }
        catch (Exception)
        {
            await auditStore.RecordAuditEntry(new AuditEntry(message, DeliveryStatus.Failed));
            throw;
        }
    }
}
