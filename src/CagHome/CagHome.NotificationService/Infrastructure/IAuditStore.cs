using CagHome.Contracts.Enums;
using CagHome.NotificationService.Domain;

namespace CagHome.NotificationService.Infrastructure
{
    public interface IAuditStore
    {
        Task RecordAuditEntry(AuditEntry entry);
    }
}
