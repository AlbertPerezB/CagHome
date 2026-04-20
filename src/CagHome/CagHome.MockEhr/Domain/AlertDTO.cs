namespace CagHome.MockEhr.Domain
{
    public record AlertDTO(
        Guid AlertId,
        Guid PatientId,
        Guid HospitalId,
        string Severity,
        string Message
    );
}
