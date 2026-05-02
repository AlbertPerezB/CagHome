using CagHome.Contracts.Enums;

public record CareplanUpdateRequested(Careplan Careplan, Guid PatientId, DateTime UpdatedAtUtc);
