using CagHome.Contracts;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;
using Microsoft.Extensions.Logging;

namespace CagHome.NotificationService.Application.Handlers;

public class PatientAlertHandler
{
    public async Task Handle(
        PatientAlertRequested message,
        MqttNotificationPublisher mqttPublisher,
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
        catch (Exception ex)
        {
            await auditStore.RecordAuditEntry(new AuditEntry(message, DeliveryStatus.Failed));
            throw;
        }
    }
}
