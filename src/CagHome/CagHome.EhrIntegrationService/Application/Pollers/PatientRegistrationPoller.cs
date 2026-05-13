using System.Diagnostics;
using System.Net.Http.Json;
using CagHome.Contracts;
using CagHome.EhrIntegrationService.Domain;
using CagHome.EhrIntegrationService.Infrastructure;
using Wolverine;

namespace CagHome.EhrIntegrationService.Application.Pollers;

public class PatientRegistrationPoller(
    IHttpClientFactory httpClientFactory,
    ILogger<PatientRegistrationPoller> logger,
    IRabbitMqPublisher publisher
) : BackgroundService
{
    private DateTime _lastPollTimestamp = DateTime.MinValue;
    private static readonly ActivitySource ActivitySource = new("CagHome.EhrIntegrationService");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Patient registration poller started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollForPatients(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to poll for patient registrations, will retry next interval"
                );
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task PollForPatients(CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("PollPatientRegistrations");

        var client = httpClientFactory.CreateClient("mock-ehr");
        var sinceParam = _lastPollTimestamp.ToString("O");

        var patients = await client.GetFromJsonAsync<List<PatientRegistrationDto>>(
            $"/patients?since={sinceParam}",
            ct
        );

        if (patients is null || patients.Count == 0)
            return;

        logger.LogInformation($"Polled {patients.Count} new patient registration(s)");

        foreach (var patient in patients)
        {
            await publisher.PublishCareplanUpdateRequested(
                new CareplanUpdateRequested(
                    patient.Careplan,
                    patient.PatientId,
                    patient.UpdatedAtUtc
                )
            );

            await publisher.PublishPatientStatusUpdateRequested(
                new PatientStatusUpdateRequested(
                    patient.PatientId,
                    patient.Status,
                    patient.UpdatedAtUtc
                )
            );
        }

        _lastPollTimestamp = patients.Max(p => p.UpdatedAtUtc);
    }
}
