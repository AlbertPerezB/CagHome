namespace CagHome.Contracts;

/// <summary>
/// Represents an ingested telemetry batch from a patient device.
/// </summary>
/// <param name="BatchId">The unique id of the batch.</param>
/// <param name="CorrelationId">The id used to correlate related events across services.</param>
/// <param name="PatientId">The unique id of the patient the batch belongs to.</param>
/// <param name="Measurements">The list of measurements contained in this batch.</param>
/// <param name="ReceivedAtUtc">The UTC timestamp when the batch was received.</param>
public record BatchReceived(
    Guid BatchId,
    Guid CorrelationId,
    Guid PatientId,
    List<MeasurementItem> Measurements,
    DateTime ReceivedAtUtc
);

/// <summary>
/// Represents a single measurement reading within a telemetry batch.
/// </summary>
/// <param name="MeasurementId">The unique id of the measurement.</param>
/// <param name="MeasurementType">The type of measurement recorded (e.g. HeartRate, Spo2).</param>
/// <param name="Value">The value of the measurement.</param>
/// <param name="Unit">The unit associated with the measurement value.</param>
/// <param name="DeviceReported">The UTC timestamp when the device reported the measurement.</param>
/// <param name="ValidationErrors">Validation errors associated with this measurement, if any.</param>
public record MeasurementItem(
    Guid MeasurementId,
    string MeasurementType,
    double Value,
    string Unit,
    DateTime DeviceReported,
    List<ValidationErrorItem> ValidationErrors
);

/// <summary>
/// Describes a validation error for a measurement item.
/// </summary>
/// <param name="Message">A description of the validation error.</param>
/// <param name="Code">The error code identifying the type of violation.</param>
public record ValidationErrorItem(string Message, string Code);
