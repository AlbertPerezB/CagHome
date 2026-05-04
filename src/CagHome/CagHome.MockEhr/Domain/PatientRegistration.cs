namespace CagHome.MockEhr.Domain
{
    public record PatientRegistrationUpdate(
        Guid PatientId,
        DateTime UpdatedAtUtc,
        Careplan Careplan,
        PatientStatus Status
    );
}
