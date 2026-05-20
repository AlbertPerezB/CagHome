namespace CagHome.MonitoringService.Domain;

/// <summary>
/// Represents the final decision state after policy evaluation, cooldown checks, and publishing attempts.
/// </summary>
/// <param name="PolicyResult">The base decision produced by policy evaluation.</param>
/// <param name="SuppressedByCooldown">Indicates whether publishing was suppressed due to cooldown.</param>
/// <param name="RemainingCooldown">The remaining cooldown duration, if suppression applied.</param>
/// <param name="PatientAlertPublished">Indicates whether the patient alert was published.</param>
/// <param name="HospitalAlertPublished">Indicates whether the hospital alert was published.</param>
/// <param name="FinalizedAtUtc">The UTC timestamp when decision was finalized completed.</param>
public record FinalDecisionResult(
    PolicyDecisionResult PolicyResult,
    bool SuppressedByCooldown,
    TimeSpan? RemainingCooldown,
    bool PatientAlertPublished,
    bool HospitalAlertPublished,
    DateTime FinalizedAtUtc
);
