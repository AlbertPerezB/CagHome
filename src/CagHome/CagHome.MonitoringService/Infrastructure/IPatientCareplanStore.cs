using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Infrastructure;

/// <summary>
/// Defines methods for patient careplan state.
/// </summary>
public interface IPatientCareplanStore
{
    /// <summary>
    /// Inserts or updates the latest careplan for a patient.
    /// </summary>
    /// <param name="patientId">The unique id of the patient.</param>
    /// <param name="careplan">The careplan value to persist.</param>
    /// <param name="updatedAtUtc">The UTC timestamp representing when the state was updated.</param>
    /// <returns>A task when the upsert operation finishes.</returns>
    Task Upsert(Guid patientId, Careplan careplan, DateTime updatedAtUtc);

    /// <summary>
    /// Attempts to retrieve the latest careplan for a patient.
    /// </summary>
    /// <param name="patientId">The unique id of the patient.</param>
    /// <returns>The careplan if found, otherwise, <see langword="null"/>.</returns>
    Task<Careplan?> TryGet(Guid patientId);
}
