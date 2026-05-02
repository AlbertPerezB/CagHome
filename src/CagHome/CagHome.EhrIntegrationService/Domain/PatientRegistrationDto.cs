using CagHome.Contracts.Enums;

namespace CagHome.EhrIntegrationService.Domain;

public record PatientRegistrationDto(
    Careplan Careplan,
    Guid PatientId,
    PatientStatus Status,
    DateTime UpdatedAtUtc
);
