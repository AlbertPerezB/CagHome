using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

public record PatientStatusUpdateRequested(
    Guid PatientId,
    PatientStatus PatientStatus,
    DateTime UpdatedAtUtc
);
