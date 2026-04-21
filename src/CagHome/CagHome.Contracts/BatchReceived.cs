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
    double Value,
    string Unit,
    DateTime DeviceReported,
    List<ValidationErrorItem> ValidationErrors
);

public record ValidationErrorItem(string Message, string Code);
