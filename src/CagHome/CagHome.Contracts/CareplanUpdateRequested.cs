using CagHome.Contracts.Enums;

public record CareplanUpdateRequested(Guid PatientId, DateTime UpdatedAtUtc, Careplan Careplan);
