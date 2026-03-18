using System.Text.Json;
using CagHome.IngestionService.Domain.Models;

public class IngestionContext
{
    public RawBatch RawBatch { get; }

    public BatchDto? BatchDto { get; set; }

    public Batch? Batch { get; set; }

    public ValidationError? FatalError { get; set; } = null;

    public JsonDocument? Json;

    public IngestionContext(RawBatch rawBatch)
    {
        RawBatch = rawBatch;
    }
}
