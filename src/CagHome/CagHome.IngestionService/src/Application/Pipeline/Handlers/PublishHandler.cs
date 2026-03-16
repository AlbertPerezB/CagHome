using System.Text.Json;
using CagHome.IngestionService.Infrastructure;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class PublishBatchHandler : IngestionHandler
{
    private readonly RabbitMqPublisher _publisher;

    public PublishBatchHandler(RabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch != null)
        {
            var json = JsonSerializer.Serialize(context.Batch);
            _publisher.PublishAsync(json);
        }
    }
}
