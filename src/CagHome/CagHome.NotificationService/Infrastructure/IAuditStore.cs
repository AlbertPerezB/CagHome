using CagHome.NotificationService.Domain;

namespace CagHome.NotificationService.Infrastructure
{
    /// <summary>
    /// Defines a contract for persisting audit entries asynchronously.
    /// </summary>
    public interface IAuditStore
    {
        Task RecordAuditEntry(AuditEntry entry);
    }
}
