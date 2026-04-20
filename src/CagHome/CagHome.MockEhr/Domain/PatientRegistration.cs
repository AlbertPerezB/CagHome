namespace CagHome.MockEhr.Domain
{
    public record PatientRegistrationUpdate(
        Guid PatientId,
        DateTime RegisteredAtUtc,
        Careplan CarePlan,
        PatientStatus Status
    );
}
