namespace CagHome.IngestionService.Domain.Models;

public abstract record Measurement
{
    public required Guid MeasurementId { get; init; }
    public required DateTime DeviceReported { get; init; }
    public required SourceInfo source { get; init; }
}
