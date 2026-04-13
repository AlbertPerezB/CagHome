namespace CagHome.Contracts.Notifications;

public record HospitalAlertRequested(
    Guid PatientId,
    Guid HospitalId,
    string Message,
    Severity Severity,
    DateTime DecidedAt
);
