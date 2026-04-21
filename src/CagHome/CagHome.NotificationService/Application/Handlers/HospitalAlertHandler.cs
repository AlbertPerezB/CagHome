using System.Net.Http.Json;
using CagHome.Contracts;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;

namespace CagHome.NotificationService.Application.Handlers;

public class HospitalAlertHandler
{
    public async Task Handle(
        HospitalAlertRequested message,
        IHttpClientFactory httpClientFactory,
        ILogger<HospitalAlertHandler> logger,
        IAuditStore auditStore
    )
    {
        var alertId = Guid.NewGuid();
        logger.LogInformation(
            "Hospital alert to be sent: "
                + "AlertID = {alertId}, PatientId={PatientId}, HospitalId={HospitalId}, Severity={Severity}, Message={Message}",
            alertId,
            message.PatientId,
            message.HospitalId,
            message.Severity,
            message.Message
        );

        await auditStore.RecordAuditEntry(new AuditEntry(message, DeliveryStatus.Attempted));

        try
        {
            var client = httpClientFactory.CreateClient("mock-ehr");
            var response = await client.PostAsJsonAsync("/alerts", message);

            response.EnsureSuccessStatusCode();
            await auditStore.RecordAuditEntry(new AuditEntry(message, DeliveryStatus.Delivered));
        }
        catch (HttpRequestException ex)
        {
            await auditStore.RecordAuditEntry(new AuditEntry(message, DeliveryStatus.Failed));
            throw;
        }
    }
}
