namespace CagHome.Contracts;

public record PatientRegistered(Guid PatientId, string Name, DateTime RegisteredAtUtc);
