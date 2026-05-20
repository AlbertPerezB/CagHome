using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

/// <summary>
/// Event raised when a patient's status has been updated.
/// </summary>
/// <param name="PatientId">The unique id of the patient.</param>
/// <param name="PatientStatus">The updated patient status.</param>
/// <param name="UpdatedAtUtc">The UTC timestamp when the status was updated.</param>
public record PatientStatusUpdated(
    Guid PatientId,
    PatientStatus PatientStatus,
    DateTime UpdatedAtUtc
);
