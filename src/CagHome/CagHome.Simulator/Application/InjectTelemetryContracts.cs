using CagHome.Simulator.Domain.Models;

namespace CagHome.Simulator.Application;

public record InjectTelemetryRequest(
    int SchemaVersion,
    string AppVersion,
    Guid PatientId,
    IReadOnlyList<MeasurementPayload> Measurements
);

public record InjectTelemetryResponse(Guid CorrelationId);
