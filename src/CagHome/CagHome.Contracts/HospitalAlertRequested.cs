namespace CagHome.Contracts;

public record HospitalAlertRequested(
    Guid PatientId,
    Guid HospitalId,
    string Message,
    Severity Severity,
    DateTime DecidedAt
);
