namespace CagHome.MonitoringService.Domain;

/// <summary>
/// Represents the outcome of cooldown validation for an alerting decision.
/// </summary>
/// <param name="IsSuppressed"> Indicates whether alerting should be suppressed because a cooldown window is still active.</param>
/// <param name="RemainingCooldown">The remaining cooldown duration, if suppression is active.</param>
public record CooldownCheckResult(
    bool IsSuppressed,
    TimeSpan? RemainingCooldown
);
