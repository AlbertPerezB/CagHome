using CagHome.Contracts.enums;

namespace CagHome.Contracts;

public record PatientAlertRequested(
    Guid PatientId,
    string Message,
    Severity Severity,
    DateTime DecidedAt
);
