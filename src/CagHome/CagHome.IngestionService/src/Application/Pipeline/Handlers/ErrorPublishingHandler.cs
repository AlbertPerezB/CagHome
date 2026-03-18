using System.Text.Json;
using CagHome.IngestionService.Infrastructure;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class ErrorPublishingHandler : IngestionHandler
{
    private readonly MqttPublisher _publisher;

    public ErrorPublishingHandler(MqttPublisher publisher)
    {
        _publisher = publisher;
    }

    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (ShouldProcess(context))
        {
            var json = JsonSerializer.Serialize(context.FatalError);
            _publisher.PublishAsync(json);
        }
    }

    public override bool ShouldProcess(IngestionContext context) => context.FatalError != null;
}
