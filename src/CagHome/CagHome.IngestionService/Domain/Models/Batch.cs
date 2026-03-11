namespace CagHome.IngestionService.Domain.Models;

public abstract record Batch
{
    public required Guid BatchId { get; init; } = new Guid();

    public required Guid PatientId { get; init; }

    public required int SchemaVerion { get; init; }

    public required Version AppVersion { get; init; }

    public required List<Measurement> Measurements { get; init; }

    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;

    public List<ValidationError> ValidationErrors { get; set; } = new();

    public ValidationError? FatalError { get; set; } = null;
}
