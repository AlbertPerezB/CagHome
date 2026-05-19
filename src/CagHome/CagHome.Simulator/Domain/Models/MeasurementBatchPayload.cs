namespace CagHome.Simulator.Domain.Models;

/// <summary>
/// Represents the root payload published by the simulator for a patient measurement batch.
/// </summary>
/// <param name="AppVersion">Version of the simulator application that produced the payload.</param>
/// <param name="CorrelationId">Correlation identifier used to trace related events across services.</param>
/// <param name="Measurements">Collection of measurements included in the batch.</param>
/// <param name="PatientId">Identifier of the patient associated with the batch.</param>
/// <param name="SchemaVersion">Payload schema version.</param>
public record MeasurementBatchPayload(
    string AppVersion,
    Guid CorrelationId,
    IReadOnlyList<MeasurementPayload> Measurements,
    Guid PatientId,
    int SchemaVersion);
