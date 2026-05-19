namespace CagHome.MonitoringService.Domain;

public record FinalDecisionResult(
    PolicyDecisionResult PolicyResult,
    bool SuppressedByCooldown,
    TimeSpan? RemainingCooldown,
    bool PatientAlertPublished,
    bool HospitalAlertPublished,
    DateTime FinalizedAtUtc
);
