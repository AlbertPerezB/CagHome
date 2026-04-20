namespace CagHome.Contracts.enums;

public record PatientStatusUpdateRequested(
    Guid PatientId,
    DateTime UpdatedAtUtc,
    PatientStatus PatientStatus
);
