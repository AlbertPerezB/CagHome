using System.Text.Json;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Domain.Models.DataTransferObjects;

/// <summary>
/// Shared state object passed through the ingestion pipeline.
/// Accumulates data as each handler processes it, progressing from
/// <see cref="RawBatch"/> to <see cref="BatchDto"/> to <see cref="Batch"/>.
/// </summary>
public class IngestionContext
{
    public RawBatch RawBatch { get; }

    public BatchDto? BatchDto { get; set; }

    public Batch? Batch { get; set; }

    public ValidationError? FatalError { get; set; }

    public JsonDocument? Json { get; set; }

    public IngestionContext(RawBatch rawBatch)
    {
        RawBatch = rawBatch;
    }
}
