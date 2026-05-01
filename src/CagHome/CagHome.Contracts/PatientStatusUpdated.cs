using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

public record PatientStatusUpdated(
    Guid PatientId,
    PatientStatus PatientStatus,
    DateTime UpdatedAtUtc
);
