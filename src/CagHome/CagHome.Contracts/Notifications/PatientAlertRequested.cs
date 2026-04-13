namespace CagHome.Contracts.Notifications;

public record PatientNotificationRequested(
    Guid PatientId,
    string Message,
    Severity Severity,
    DateTime DecidedAt
);
