using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application;

public class IngestionService : IIngestionService
{
    private readonly IIngestionHandler PipelineRoot;

    public IngestionService(IIngestionHandler pipelineRoot)
    {
        PipelineRoot = pipelineRoot;
    }

    public async Task ProcessAsync(RawBatch rawBatch)
    {
        var context = new IngestionContext(rawBatch);

        await PipelineRoot.HandleAsync(context);
    }
}
