namespace CagHome.MockEhr.Domain
{
    public record ClinicianResponse(
        Guid ResponseId,
        Guid AlertId,
        Guid PatientId,
        string Message,
        DateTime CreatedAtUtc
    );
}
