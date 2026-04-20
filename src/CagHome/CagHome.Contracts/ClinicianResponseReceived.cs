namespace CagHome.Contracts;

public record ClinicianResponseReceived(
    Guid ResponseId,
    Guid AlertId,
    Guid PatientId,
    string Message,
    DateTime ReceivedAtUtc
);
