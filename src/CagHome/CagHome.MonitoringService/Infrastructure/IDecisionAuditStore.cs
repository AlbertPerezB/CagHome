using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Infrastructure;

/// <summary>
/// Defines method for recording monitoring decision audit entries.
/// </summary>
public interface IDecisionAuditStore
{
    /// <summary>
    /// Records a decision audit entry.
    /// </summary>
    /// <param name="entry">The audit entry to record.</param>
    /// <returns>A task that completes when the entry is stored.</returns>
    Task RecordAuditEntry(DecisionAuditEntry entry);
}
