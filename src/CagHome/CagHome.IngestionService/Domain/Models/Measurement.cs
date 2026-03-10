using CagHome.IngestionService.Domain.Enums;

namespace CagHome.IngestionService.Domain.Models;

public abstract record Measurement
{
    public required Guid MeasurementId { get; init; }
    public required MeasurementType MeasurementType { get; init; }
    public required float Value { get; init; }
    public required string unit { get; init; }
    public required DateTime DeviceReported { get; init; }
    public required DeviceInfo source { get; init; }
    public ValidationOutcome ValidationOutcome { get; set; } = new();
}
