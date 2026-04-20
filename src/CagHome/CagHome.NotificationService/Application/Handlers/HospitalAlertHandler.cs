using System.Net.Http.Json;
using CagHome.Contracts;
using Microsoft.Extensions.Logging;

namespace CagHome.NotificationService.Application.Handlers;

public class HospitalAlertHandler
{
    public async Task Handle(
        HospitalAlertRequested message,
        IHttpClientFactory httpClientFactory,
        ILogger<HospitalAlertHandler> logger
    )
    {
        var alertId = Guid.NewGuid();
        logger.LogInformation(
            "Hospital alert to be sent: AlertID = {alertId}, PatientId={PatientId}, HospitalId={HospitalId}, Severity={Severity}, Message={Message}",
            alertId,
            message.PatientId,
            message.HospitalId,
            message.Severity,
            message.Message
        );

        var client = httpClientFactory.CreateClient("mock-ehr");
        var response = await client.PostAsJsonAsync("/alerts", message);

        response.EnsureSuccessStatusCode();

        return Task.CompletedTask;
    }
}
