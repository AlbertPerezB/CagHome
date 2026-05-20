using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Application.Decision.Interfaces;

/// <summary>
/// Evaluates cooldown constraints to determine whether alerts should be suppressed.
/// </summary>
public interface ICooldownService
{
    /// <summary>
    /// Evaluates cooldown status for a patient and severity at a specific time.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient.</param>
    /// <param name="severity">The alert severity being considered.</param>
    /// <param name="timestampUtc">The UTC timestamp at which cooldown is evaluated.</param>
    /// <returns>The cooldown evaluation result, including suppression status and remaining duration.</returns>
    CooldownCheckResult Evaluate(Guid patientId, Severity severity, DateTime timestampUtc);
}
