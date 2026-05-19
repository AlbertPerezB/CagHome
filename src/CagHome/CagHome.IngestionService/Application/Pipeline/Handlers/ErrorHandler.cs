using System.Text.Json;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

/// <summary>
/// Handler to log fatal errors that occur during ingestion. This is the last handler in the pipeline.
/// </summary>
public class ErrorHandler(ILogger<ErrorHandler> logger) : IngestionHandler
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
