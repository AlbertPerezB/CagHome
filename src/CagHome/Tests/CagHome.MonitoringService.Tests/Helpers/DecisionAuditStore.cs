using CagHome.MonitoringService.Domain;
using CagHome.MonitoringService.Infrastructure;

namespace CagHome.MonitoringService.Tests.Helpers;

public sealed class DecisionAuditStore : IDecisionAuditStore
{
    private readonly object _lock = new();
    private readonly List<DecisionAuditEntry> _entries = [];

    public Task RecordAuditEntry(DecisionAuditEntry entry)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }

        return Task.CompletedTask;
    }

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

    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }
}
