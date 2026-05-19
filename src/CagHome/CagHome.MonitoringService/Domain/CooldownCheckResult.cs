namespace CagHome.MonitoringService.Domain;

public record CooldownCheckResult(
    bool IsSuppressed,
    TimeSpan? RemainingCooldown
);
