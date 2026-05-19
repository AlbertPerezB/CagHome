using System.Collections.Concurrent;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Infrastructure;

namespace CagHome.MonitoringService.Tests.Helpers;

/// <summary>
/// In-memory test implementation of <see cref="IPatientCareplanStore"/>.
/// </summary>
public class PatientCareplanStore : IPatientCareplanStore
{
    private readonly ConcurrentDictionary<Guid, (Careplan Careplan, DateTime UpdatedAtUtc)> _careplans =
        new();

    /// <summary>
    /// Inserts or updates the current careplan for a patient.
    /// </summary>
    /// <param name="patientId">The patient id.</param>
    /// <param name="careplan">The careplan to store.</param>
    /// <param name="updatedAtUtc">The UTC timestamp indicating when the careplan was last updated.</param>
    /// <returns>A completed task.</returns>
    public Task Upsert(Guid patientId, Careplan careplan, DateTime updatedAtUtc)
    {
        _careplans[patientId] = (careplan, updatedAtUtc);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to retrieve the current careplan for a patient.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <returns>
    /// A task that resolves to the patient's <see cref="Careplan"/>, or <see langword="null"/> when none exists.
    /// </returns>
    public Task<Careplan?> TryGet(Guid patientId)
    {
        var exists = _careplans.TryGetValue(patientId, out var value);
        return Task.FromResult(exists ? value.Careplan : (Careplan?)null);
    }

    /// <summary>
    /// Removes all stored patient careplans.
    /// </summary>
    public void Clear()
    {
        _careplans.Clear();
    }
}
