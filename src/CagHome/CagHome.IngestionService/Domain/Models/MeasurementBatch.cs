namespace CagHome.IngestionService.Domain.Models;

public abstract record MeasurementBatch
{
    public required Guid PatientId { get; init; }
    public required Version SchemaVerion { get; init; }
    public required Version AppVersion { get; init; }
    public required List<Measurement> Measurements { get; init; }
}
