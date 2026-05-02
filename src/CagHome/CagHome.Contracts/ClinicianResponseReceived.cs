namespace CagHome.Contracts;

public record ClinicianResponseReceived(
    Guid AlertId,
    DateTime CreatedAtUtc,
    Guid HospitalId,
    string Message,
    Guid ResponseId,
    Guid PatientId
);
