using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Infrastructure;

public interface IDecisionAuditStore
{
    Task RecordAuditEntry(DecisionAuditEntry entry);
}
