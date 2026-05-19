namespace CagHome.MockEhr.Domain
{
    public record ClinicianResponse(
        Guid AlertId,
        DateTime CreatedAtUtc,
        Guid CorrelationId,
        Guid HospitalId,
        string Message,
        Guid PatientId,
        Guid ResponseId
    );
}
