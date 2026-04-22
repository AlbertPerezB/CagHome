using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

public record PatientAlertRequested(
    Guid AlertId,
    DateTime DecidedAt,
    string Message,
    Guid PatientId,
    Severity Severity
);
