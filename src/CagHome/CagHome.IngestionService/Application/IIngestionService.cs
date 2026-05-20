using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application;

/// <summary>
/// Defines the contract for the ingestion service, which processes raw batches of data and transforms
/// them into a structured format for further processing.
/// </summary>
public interface IIngestionService
{
    Task<IngestionContext> ProcessAsync(RawBatch rawBatch);
}
