using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

public record HospitalAlertRequested(
    Guid AlertId,
    Guid CorrelationId,
    DateTime DecidedAt,
    Guid HospitalId,
    string Message,
    Guid PatientId,
    Severity Severity
);
