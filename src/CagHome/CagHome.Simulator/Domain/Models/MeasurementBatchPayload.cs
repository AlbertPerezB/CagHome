namespace CagHome.Simulator.Domain.Models;

public record MeasurementBatchPayload(
    string AppVersion,
    Guid CorrelationId,
    IReadOnlyList<MeasurementPayload> Measurements,
    Guid PatientId,
    int SchemaVersion);
