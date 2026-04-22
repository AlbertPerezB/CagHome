namespace CagHome.Simulator.Domain.Models;

public sealed record MeasurementPayload(
    DateTimeOffset DeviceReported,
    Guid MeasurementId,
    MeasurementSourcePayload Source,
    string Type,
    string Unit,
    double Value);