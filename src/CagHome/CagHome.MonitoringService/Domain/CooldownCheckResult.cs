namespace CagHome.MonitoringService.Domain;

public sealed record CooldownCheckResult(
    bool IsSuppressed,
    TimeSpan? RemainingCooldown
);
