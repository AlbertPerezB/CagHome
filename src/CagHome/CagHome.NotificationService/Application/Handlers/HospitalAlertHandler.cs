using System.Net;
using System.Net.Http.Json;
using CagHome.Contracts;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;
using Microsoft.AspNetCore.Http;

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

        var client = httpClientFactory.CreateClient("mock-ehr");
        var response = await client.PostAsJsonAsync("/alerts", message);

        if (!response.IsSuccessStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            await auditStore.RecordAuditEntry(
                new AuditEntry(message, DeliveryStatus.Failed, statusCode.ToString())
            );

            if (statusCode >= 500)
            {
                throw new HttpRequestException($"EHR returned {response.StatusCode}");
            }

            throw new BadHttpRequestException(
                $"EHR rejected alert: {response.StatusCode}",
                statusCode
            );
        }
        await auditStore.RecordAuditEntry(
            new AuditEntry(message, DeliveryStatus.Delivered, response.StatusCode.ToString())
        );
    }
}
