namespace CagHome.IngestionService.Domain.Models;

public record Batch
{
    public required Guid BatchId { get; init; } = new Guid();

    public required Guid PatientId { get; init; }

    public required int SchemaVersion { get; init; }

    public required Version AppVersion { get; init; }

    public required List<Measurement> Measurements { get; init; }

    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;

    public List<ValidationError> ValidationErrors { get; set; } = new();

    public ValidationError? FatalError { get; set; } = null;
}
