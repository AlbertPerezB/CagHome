using CagHome.MonitoringService.Domain;
using CagHome.MonitoringService.Infrastructure;

namespace CagHome.MonitoringService.Tests.Helpers;

/// <summary>
/// In-memory test implementation of <see cref="IDecisionAuditStore"/>.
/// </summary>
public class DecisionAuditStore : IDecisionAuditStore
{
    private readonly object _lock = new();
    private readonly List<DecisionAuditEntry> _entries = [];

    /// <summary>
    /// Records an audit entry.
    /// </summary>
    /// <param name="entry">The audit entry to record.</param>
    /// <returns>A completed task.</returns>
    public Task RecordAuditEntry(DecisionAuditEntry entry)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a snapshot of all recorded audit entries.
    /// </summary>
    public IReadOnlyList<DecisionAuditEntry> Entries
    {
        get
        {
            lock (_lock)
            {
                return _entries.ToList();
            }
        }
    }

    /// <summary>
    /// Clears all recorded audit entries.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }
}
