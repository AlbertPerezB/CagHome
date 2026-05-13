namespace CagHome.MockEhr.Domain
{
    public record AlertDTO(
        Guid AlertId,
        Guid CorrelationId,
        DateTime DecidedAt,
        Guid HospitalId,
        string Message,
        Guid PatientId,
        int Severity
    );
}
