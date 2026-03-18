namespace CagHome.Simulator;

public sealed record MeasurementBatchPayload(
    int SchemaVersion,
    decimal AppVersion,
    Guid PatientId,
    IReadOnlyList<MeasurementPayload> Measurements);

public sealed record MeasurementPayload(
    Guid MeasurementId,
    string Type,
    double Value,
    string Unit,
    DateTimeOffset DeviceReported,
    MeasurementSourcePayload Source);

public sealed record MeasurementSourcePayload(
    string DeviceManufacturer,
    string DeviceModel,
    string Platform);
