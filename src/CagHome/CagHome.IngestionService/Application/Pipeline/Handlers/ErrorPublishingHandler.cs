using System.Text.Json;
using CagHome.IngestionService.Infrastructure;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class ErrorPublishingHandler(ILogger<ErrorPublishingHandler> logger) : IngestionHandler
{
    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (ShouldProcess(context))
        {
            logger.LogError($"FatalError: {context!.FatalError!.Message}");
            var json = JsonSerializer.Serialize(context.FatalError);
        }
    }

    public override bool ShouldProcess(IngestionContext context) => context.FatalError != null;
}
