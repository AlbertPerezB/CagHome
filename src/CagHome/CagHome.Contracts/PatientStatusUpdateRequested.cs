using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

public record PatientStatusUpdateRequested(
    Guid PatientId,
    DateTime UpdatedAtUtc,
    PatientStatus PatientStatus
);
