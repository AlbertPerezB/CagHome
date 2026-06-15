using System.Runtime.CompilerServices;
using CagHome.MockApplication.Domain.Models;
using CagHome.MockApplication.Domain.Profiles;

namespace CagHome.MockApplication.Application;

/// <summary>
/// Class responsible for registering patients through the EHR mock API interface.
/// </summary>
/// <param name="httpClientFactory">The httpClientFactory used to create a http client. </param>
/// <param name="logger">The logger to log information and errors. </param>
public sealed class PatientRegistrationService(
    IHttpClientFactory httpClientFactory,
    ILogger<PatientRegistrationService> logger
)
{
    public async Task<List<Guid>> RegisterAsync(int PatientCount, CancellationToken ct)
    {
        var patientIds = new List<Guid>();
        for (var i = 0; i < PatientCount; i++)
        {
            var patientId = Guid.NewGuid();
            await RegisterPatient(patientId, ct);
            patientIds.Add(patientId);
        }
        return patientIds;
    }

    private async Task RegisterPatient(Guid patientId, CancellationToken ct)
    {
        var payload = new
        {
            patientId,
            updatedAtUtc = DateTime.UtcNow,
            careplan = Careplan.ValveDisease,
            status = PatientStatus.Active,
        };

        try
        {
            var client = httpClientFactory.CreateClient("mock-ehr");
            var response = await client.PostAsJsonAsync("/mock/patient", payload, ct);
            response.EnsureSuccessStatusCode();

            logger.LogDebug(
                "Registered patient {Id} in EHR — status: {Status}, careplan: {Careplan}",
                patientId,
                payload.status,
                payload.careplan
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to register patient {Id} in EHR, they will be rejected by ingestion",
                patientId
            );
        }
    }
}
