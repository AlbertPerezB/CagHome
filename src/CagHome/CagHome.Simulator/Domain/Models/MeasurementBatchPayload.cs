namespace CagHome.Simulator.Domain.Models;

public sealed record MeasurementBatchPayload(
    string AppVersion,
    IReadOnlyList<MeasurementPayload> Measurements,
    Guid PatientId,
    int SchemaVersion);

public sealed record MeasurementPayload(
    DateTimeOffset DeviceReported,
    Guid MeasurementId,
    MeasurementSourcePayload Source,
    string Type,
    string Unit,
    double Value);

public sealed record MeasurementSourcePayload(
    string DeviceId,
    string DeviceManufacturer,
    string DeviceModel);
