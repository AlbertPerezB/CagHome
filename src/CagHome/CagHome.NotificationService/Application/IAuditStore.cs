using CagHome.Contracts.Enums;

namespace CagHome.NotificationService.Application
{
    public interface IAuditStore
    {
        Task RecordAlertAttempted(
            Guid patientId,
            Guid hospitalId,
            Severity severity,
            string message,
            DateTime timestamp
        );

        Task RecordAlertDelivered(Guid patientId, int statusCode, DateTime timestamp);
    }
}
