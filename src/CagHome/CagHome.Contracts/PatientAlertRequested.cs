using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

public record PatientAlertRequested(
    Guid AlertId,
    Guid CorrelationId,
    DateTime DecidedAt,
    string Message,
    Guid PatientId,
    Severity Severity
);
