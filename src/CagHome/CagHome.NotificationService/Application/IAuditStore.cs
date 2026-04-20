using System;
using System.Collections.Generic;
using System.Text;
using CagHome.Contracts.enums;

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
