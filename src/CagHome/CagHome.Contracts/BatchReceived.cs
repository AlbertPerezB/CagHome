namespace CagHome.Contracts;

public record BatchReceived(
    Guid BatchId,
    Guid PatientId,
    List<MeasurementItem> Measurements,
    DateTime ReceivedAtUtc
);

public record MeasurementItem(
    Guid MeasurementId,
    string MeasurementType,
    float Value,
    string Unit,
    string DeviceId,
    DateTime DeviceReported
);
