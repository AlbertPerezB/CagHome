using CagHome.Contracts.enums;

public record CareplanUpdateRequested(Guid PatientId, DateTime UpdatedAtUtc, Careplan Careplan);
