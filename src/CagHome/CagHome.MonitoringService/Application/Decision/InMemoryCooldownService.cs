using System.Collections.Concurrent;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Application.Decision;

public sealed class InMemoryCooldownService : ICooldownService
{
    private readonly ConcurrentDictionary<string, DateTime> _lastAlertByPatientAndSeverity = new();

    public CooldownCheckResult Evaluate(Guid patientId, Severity severity, DateTime timestampUtc)
    {
        var cooldown = GetCooldownWindow(severity);
        if (cooldown <= TimeSpan.Zero)
        {
            return new CooldownCheckResult(IsSuppressed: false, RemainingCooldown: null);
        }

        var key = $"{patientId:N}:{severity}";
        if (_lastAlertByPatientAndSeverity.TryGetValue(key, out var previousAlertAtUtc))
        {
            var elapsed = timestampUtc - previousAlertAtUtc;
            if (elapsed < cooldown)
            {
                return new CooldownCheckResult(
                    IsSuppressed: true,
                    RemainingCooldown: cooldown - elapsed
                );
            }
        }

        _lastAlertByPatientAndSeverity[key] = timestampUtc;
        return new CooldownCheckResult(IsSuppressed: false, RemainingCooldown: null);
    }

    private static TimeSpan GetCooldownWindow(Severity severity)
    {
        return severity switch
        {
            Severity.Warning => TimeSpan.FromMinutes(15),
            Severity.Critical => TimeSpan.FromMinutes(5),
            _ => TimeSpan.Zero,
        };
    }
}
