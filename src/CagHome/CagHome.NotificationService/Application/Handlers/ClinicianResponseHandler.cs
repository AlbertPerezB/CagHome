using CagHome.Contracts;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;
using Microsoft.Extensions.Logging;

namespace CagHome.NotificationService.Application.Handlers;

public class ClinicianResponseHandler
{
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
