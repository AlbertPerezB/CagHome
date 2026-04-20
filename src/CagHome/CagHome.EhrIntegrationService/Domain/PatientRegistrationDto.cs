using CagHome.Contracts.Enums;

namespace CagHome.EhrIntegrationService.Domain;

public record PatientRegistrationDto(
    Guid PatientId,
    DateTime UpdatedAtUtc,
    Careplan Careplan,
    PatientStatus Status
);
