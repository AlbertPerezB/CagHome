using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public interface IIngestionHandler
{
    IIngestionHandler SetNext(IIngestionHandler next);
    Task HandleAsync(IngestionContext context);
}
