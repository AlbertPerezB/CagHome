namespace CagHome.EhrIntegrationService.Domain;

public record ClinicianResponseDto(
    Guid AlertId,
    DateTime CreatedAtUtc,
    Guid CorrelationId,
    Guid HospitalId,
    string Message,
    Guid PatientId,
    Guid ResponseId
);
