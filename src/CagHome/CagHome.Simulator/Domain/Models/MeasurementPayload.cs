namespace CagHome.Simulator.Domain.Models;

public record MeasurementPayload(
    DateTimeOffset DeviceReported,
    Guid MeasurementId,
    MeasurementSourcePayload Source,
    string Type,
    string Unit,
    double Value);