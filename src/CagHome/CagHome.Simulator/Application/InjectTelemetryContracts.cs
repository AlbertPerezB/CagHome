using CagHome.Simulator.Domain.Models;

namespace CagHome.Simulator.Application;

/// <summary>
/// Represents a request to publish a telemetry batch for a specific patient.
/// </summary>
/// <param name="SchemaVersion">Schema version of the telemetry payload.</param>
/// <param name="AppVersion">Application version producing the payload.</param>
/// <param name="PatientId">Identifier of the patient receiving the telemetry data.</param>
/// <param name="Measurements">Telemetry measurements included in the batch.</param>
public record InjectTelemetryRequest(
    int SchemaVersion,
    string AppVersion,
    Guid PatientId,
    IReadOnlyList<MeasurementPayload> Measurements
);

/// <summary>
/// Represents the result of an injected telemetry publish operation.
/// </summary>
/// <param name="CorrelationId">Correlation id assigned to the published batch.</param>
public record InjectTelemetryResponse(Guid CorrelationId);
