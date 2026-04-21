using System.Net.Http.Json;
using CagHome.Contracts;
using CagHome.EhrIntegrationService.Domain;
using CagHome.EhrIntegrationService.Infrastructure;
using Wolverine;

namespace CagHome.EhrIntegrationService.Application.Pollers;

public class ClinicianResponsePoller(
    IHttpClientFactory httpClientFactory,
    ILogger<ClinicianResponsePoller> logger,
    IRabbitMqPublisher publisher
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
        foreach (var response in responses)
        {
            await publisher.PublishClinicianResponseReceived(
                new ClinicianResponseReceived(
                    response.ResponseId,
                    response.AlertId,
                    response.PatientId,
                    response.Message,
                    response.CreatedAtUtc
                )
            );
            logger.LogInformation(
                $"Received clinician response {response.ResponseId}"
                    + $" for alert {response.AlertId} and patient {response.PatientId}: {response.Message}"
            );
        }
        logger.LogInformation($"Polled {responses.Count} clinician response(s)");

        _lastPollTimestamp = responses.Max(r => r.CreatedAtUtc);
    }
}
