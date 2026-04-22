using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

public record HospitalAlertRequested(
    Guid AlertId,
    DateTime DecidedAt,
    Guid HospitalId,
    string Message,
    Guid PatientId,
    Severity Severity
);
