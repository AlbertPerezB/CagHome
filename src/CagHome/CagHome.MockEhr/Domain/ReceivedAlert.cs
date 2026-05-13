namespace CagHome.MockEhr.Domain
{
    public record ReceivedAlert(
        Guid AlertId,
        Guid PatientId,
        Guid HospitalId,
        string Message,
        Severity Severity,
        DateTime ReceivedAtUtc
    );
}
