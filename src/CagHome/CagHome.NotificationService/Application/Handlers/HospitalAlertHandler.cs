using System.Net.Http.Json;
using CagHome.Contracts;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace CagHome.NotificationService.Application.Handlers;

/// <summary>
/// Handles hospital alert requests by sending alert messages to the external EHR system, logging the operation, and
/// recording audit entries for each delivery attempt.
/// </summary>
public class HospitalAlertHandler
{
    /// <summary>
    /// Processes a hospital alert request by sending the alert to the external EHR system, recording audit entries for
    /// each delivery attempt and outcome.
    /// </summary>
    /// <remarks>Audit entries are recorded for each delivery attempt, failure, and successful delivery. The
    /// method logs alert details for monitoring and troubleshooting purposes.</remarks>
    /// <param name="message">The hospital alert request message containing alert details to be sent.</param>
    /// <param name="httpClientFactory">The HTTP client factory used to create a client for communicating with the external EHR system.</param>
    /// <param name="logger">The logger used to record informational messages about the alert delivery process.</param>
    /// <param name="auditStore">The audit store used to record audit entries for each alert delivery attempt and result.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="HttpRequestException">Thrown if the external EHR system returns a server error (HTTP status code 500 or greater).</exception>
    /// <exception cref="BadHttpRequestException">Thrown if the external EHR system rejects the alert with a non-success status code below 500.</exception>
    public async Task Handle(
        HospitalAlertRequested message,
        IHttpClientFactory httpClientFactory,
        ILogger<HospitalAlertHandler> logger,
        IAuditStore auditStore
    )
    {
        logger.LogInformation(
            "Hospital alert to be sent: "
                + "AlertID = {alertId}, PatientId={PatientId}, HospitalId={HospitalId}, Severity={Severity}, Message={Message}",
            message.AlertId,
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
