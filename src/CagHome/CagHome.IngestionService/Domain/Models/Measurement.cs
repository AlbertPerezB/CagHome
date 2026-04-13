using System.Collections.Concurrent;
using CagHome.IngestionService.Domain.Enums;

namespace CagHome.IngestionService.Domain.Models;

public record Measurement
{
    public required Guid MeasurementId { get; init; }

    public required MeasurementType MeasurementType { get; init; }

    public required double Value { get; init; }

    public required Unit Unit { get; init; }

    public required DateTime DeviceReported { get; init; }

    public required DeviceInfo Source { get; init; }

    public ConcurrentBag<ValidationError> ValidationErrors { get; set; } = new();
}
