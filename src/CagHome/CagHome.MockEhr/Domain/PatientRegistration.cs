namespace CagHome.MockEhr.Domain
{
    public record PatientRegistration(Guid PatientId, DateTime RegisteredAtUtc, CarePlan CarePlan);
}
