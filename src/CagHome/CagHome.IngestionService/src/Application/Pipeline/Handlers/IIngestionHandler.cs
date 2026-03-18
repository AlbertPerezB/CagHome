using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public interface IIngestionHandler
{
    IIngestionHandler SetNext(IIngestionHandler next);
    bool ShouldProcess(IngestionContext context);
    Task HandleAsync(IngestionContext context);
}
