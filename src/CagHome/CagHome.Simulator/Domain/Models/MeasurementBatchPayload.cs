namespace CagHome.Simulator.Domain.Models;

public sealed record MeasurementBatchPayload(
    string AppVersion,
    IReadOnlyList<MeasurementPayload> Measurements,
    Guid PatientId,
    int SchemaVersion);
