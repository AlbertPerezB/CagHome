using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

/// <summary>
/// Message requesting that a patient's status be updated.
/// </summary>
/// <param name="PatientId">The unique id of the patient.</param>
/// <param name="PatientStatus">The new status to apply to the patient.</param>
/// <param name="UpdatedAtUtc">The UTC timestamp when the status was updated in the EHR system.</param>
public record PatientStatusUpdateRequested(
    Guid PatientId,
    PatientStatus PatientStatus,
    DateTime UpdatedAtUtc
);
