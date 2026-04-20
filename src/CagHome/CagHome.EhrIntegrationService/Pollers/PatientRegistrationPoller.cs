using System.Net.Http.Json;
using CagHome.Contracts;
using Wolverine;

namespace CagHome.EhrIntegrationService.Infrastructure;

public class PatientRegistrationPoller(
    IHttpClientFactory httpClientFactory,
    IMessageBus messageBus,
    ILogger<PatientRegistrationPoller> logger
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

        logger.LogInformation("Polled {Count} new patient registration(s)", patients.Count);

        foreach (var patient in patients)
        {
            await messageBus.PublishAsync(
                new PatientRegistered(
                    PatientId: patient.PatientId,
                    Name: patient.Name,
                    RegisteredAtUtc: DateTime.UtcNow
                )
            );

            logger.LogInformation(
                "Published PatientRegistered: PatientId={PatientId}, Name={Name}",
                patient.PatientId,
                patient.Name
            );
        }

        _lastPollTimestamp = patients.Max(p => p.RegisteredAtUtc);
    }
}

/// <summary>
/// DTO matching the shape returned by Mock EHR's GET /patients.
/// </summary>
public record PatientRegistrationDto(Guid PatientId, string Name, DateTime RegisteredAtUtc);
