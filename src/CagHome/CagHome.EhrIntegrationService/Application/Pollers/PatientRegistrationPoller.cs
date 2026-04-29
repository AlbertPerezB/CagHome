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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Patient registration poller started");

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

            logger.LogInformation(
                "Published PatientRegistered: PatientId={PatientId}, Careplan={Careplan}, Status={Status}",
                patient.Careplan,
                patient.PatientId,
                patient.Status
            );
        }

        _lastPollTimestamp = patients.Max(p => p.UpdatedAtUtc);
    }
}
