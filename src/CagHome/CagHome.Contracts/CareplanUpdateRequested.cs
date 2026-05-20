using CagHome.Contracts.Enums;

/// <summary>
/// Message requesting that a patient's assigned careplan be updated.
/// </summary>
/// <param name="Careplan">The new careplan to assign to the patient.</param>
/// <param name="PatientId">The unique id of the patient.</param>
/// <param name="UpdatedAtUtc">The UTC timestamp when the update was requested.</param>
public record CareplanUpdateRequested(Careplan Careplan, Guid PatientId, DateTime UpdatedAtUtc);
