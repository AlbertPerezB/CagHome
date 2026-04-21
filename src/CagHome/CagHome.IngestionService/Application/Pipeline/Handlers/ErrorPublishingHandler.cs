using System.Text.Json;
using CagHome.IngestionService.Infrastructure;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class ErrorPublishingHandler(MqttPublisher publisher, ILogger<ErrorPublishingHandler> logger)
    : IngestionHandler
{
    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (ShouldProcess(context))
        {
            logger.LogError($"FatalError: {context!.FatalError!.Message}");
            var json = JsonSerializer.Serialize(context.FatalError);
            publisher.PublishAsync(json);
        }
    }

    public override bool ShouldProcess(IngestionContext context) => context.FatalError != null;
}
