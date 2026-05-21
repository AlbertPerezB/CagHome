using CagHome.Contracts;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;

namespace CagHome.NotificationService.Application.Handlers;

/// <summary>
/// Handler invoken when a <see cref="ClinicianResponseReceived"/> message arrives. Publishes the clincian response to
/// the patient and maintains an audit trail.
/// </summary>
public class ClinicianResponseHandler
{
    /// <summary>
    /// Processes a clinician response message by publishing it to the MQTT broker and recording the delivery status in
    /// the audit store.
    /// </summary>
    /// <remarks>An audit entry is recorded for each delivery attempt, successful delivery, or failure. If
    /// publishing fails, the message is sent to the dead-letter queue.</remarks>
    /// <param name="message">The clinician response message to be processed.</param>
    /// <param name="mqttPublisher">The MQTT publisher used to send the message to the appropriate patient channel.</param>
    /// <param name="auditStore">The audit store used to record the delivery status of the message.</param>
    /// <param name="logger">The logger used to record diagnostic information and errors related to the message processing.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Handle(
        ClinicianResponseReceived message,
        IMqttPublisher mqttPublisher,
        IAuditStore auditStore,
        ILogger<ClinicianResponseReceived> logger
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
