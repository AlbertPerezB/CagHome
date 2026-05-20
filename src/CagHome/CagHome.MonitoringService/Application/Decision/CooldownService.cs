using System.Collections.Concurrent;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Application.Decision;

/// <summary>
/// Applies cooldown rules to suppress repeated alerts for the same patient and severity.
/// </summary>
public class CooldownService : ICooldownService
{
    private readonly ConcurrentDictionary<string, DateTime> _lastAlertByPatientAndSeverity = new();

    /// <summary>
    /// Evaluates whether alert publication should be suppressed by cooldown.
    /// </summary>
    /// <param name="patientId">The unique id of the patient.</param>
    /// <param name="severity">The severity of the alert being evaluated.</param>
    /// <param name="timestampUtc">The UTC timestamp for the current evaluation.</param>
    /// <returns>The cooldown evaluation result with suppression state and remaining duration.</returns>
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

    /// <summary>
    /// Gets the cooldown window configured for a severity level.
    /// </summary>
    /// <param name="severity">The alert severity.</param>
    /// <returns>The cooldown duration for the specified severity.</returns>
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
