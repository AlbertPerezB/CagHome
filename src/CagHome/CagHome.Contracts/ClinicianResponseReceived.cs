namespace CagHome.Contracts;

public record ClinicianResponseReceived(
    Guid AlertId,
    DateTime CreatedAtUtc,
    Guid CorrelationId,
    Guid HospitalId,
    string Message,
    Guid PatientId,
    Guid ResponseId
);
