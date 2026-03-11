using CagHome.IngestionService.Domain.Models;

public class IngestionContext
{
    public RawBatch RawBatch { get; }

    public Batch? Batch { get; set; }

    public ValidationError? fatalError { get; set; } = null;

    public IngestionContext(RawBatch rawBatch)
    {
        RawBatch = rawBatch;
    }
}
