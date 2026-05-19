namespace CagHome.MockEhr.Domain
{
    public record ClinicianResponseDto(
        Guid AlertId,
        DateTime CreatedAtUtc,
        Guid HospitalId,
        string Message,
        Guid PatientId
    );
}
