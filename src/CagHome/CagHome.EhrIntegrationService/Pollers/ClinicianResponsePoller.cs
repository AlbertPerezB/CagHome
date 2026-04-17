using System.Net.Http.Json;
using CagHome.Contracts;
using Wolverine;

namespace CagHome.EhrIntegrationService.Infrastructure;

public class ClinicianResponsePoller(
    IHttpClientFactory httpClientFactory,
    IMessageBus messageBus,
    ILogger<ClinicianResponsePoller> logger
) : BackgroundService
{
    private DateTime _lastPollTimestamp = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Clinician response poller started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollForResponses(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to poll for clinician responses, will retry next interval"
                );
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task PollForResponses(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("mock-ehr");
        var sinceParam = _lastPollTimestamp.ToString("O");

        var responses = await client.GetFromJsonAsync<List<ClinicianResponseDto>>(
            $"/clinician-responses?since={sinceParam}",
            ct
        );

        if (responses is null || responses.Count == 0)
            return;

        logger.LogInformation("Polled {Count} clinician response(s)", responses.Count);

        foreach (var response in responses)
        {
            await messageBus.PublishAsync(
                new ClinicianResponseReceived(
                    ResponseId: response.ResponseId,
                    AlertId: response.AlertId,
                    PatientId: response.PatientId,
                    Message: response.Message,
                    ReceivedAtUtc: DateTime.UtcNow
                ),
                ct
            );

            logger.LogInformation(
                "Published ClinicianResponseReceived: ResponseId={ResponseId}, AlertId={AlertId}",
                response.ResponseId,
                response.AlertId
            );
        }

        _lastPollTimestamp = responses.Max(r => r.CreatedAtUtc);
    }
}

/// <summary>
/// DTO matching the shape returned by Mock EHR's GET /clinician-responses.
/// This is deliberately separate from the internal contract — it mirrors
/// the external API, not our domain.
/// </summary>
public record ClinicianResponseDto(
    Guid ResponseId,
    Guid AlertId,
    Guid PatientId,
    string Message,
    DateTime CreatedAtUtc
);
