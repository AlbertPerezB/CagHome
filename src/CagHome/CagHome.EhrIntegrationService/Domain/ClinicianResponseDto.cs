namespace CagHome.EhrIntegrationService.Domain;

public record ClinicianResponseDto(
    Guid ResponseId,
    Guid AlertId,
    Guid PatientId,
    string Message,
    DateTime CreatedAtUtc
);
