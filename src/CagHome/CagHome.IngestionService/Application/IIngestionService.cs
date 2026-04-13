using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application;

public interface IIngestionService
{
    Task<IngestionContext> ProcessAsync(RawBatch rawBatch);
}
