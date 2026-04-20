namespace CagHome.MockEhr.Domain
{
    public record ReceivedAlert(
        Guid AlertId,
        Guid PatientId,
        Guid HospitalId,
        string Message,
        string Severity,
        DateTime ReceivedAtUtc
    );
}
