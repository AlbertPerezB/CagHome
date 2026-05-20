namespace CagHome.MockApplication.Domain.Models;

/// <summary>
/// Represents a single (normalized) measurement item in a simulator batch payload.
/// </summary>
/// <param name="DeviceReported">Timestamp reported by the device for when the measurement was captured.</param>
/// <param name="MeasurementId">Unique identifier for the measurement event.</param>
/// <param name="Source">Metadata about the device that produced the measurement.</param>
/// <param name="Type">Measurement type name, such as HeartRate or Temperature.</param>
/// <param name="Unit">Unit associated with <paramref name="Value"/>, such as bpm or C.</param>
/// <param name="Value">Numeric value of the measurement.</param>
public record MeasurementPayload(
    DateTimeOffset DeviceReported,
    Guid MeasurementId,
    MeasurementSourcePayload Source,
    string Type,
    string Unit,
    double Value);