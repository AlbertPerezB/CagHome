using System.Text.Json;
using CagHome.IngestionService.Infrastructure;
using CagHome.IngestionService.Infrastructure.Messaging;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class PublishBatchHandler : IngestionHandler
{
    private readonly RabbitMqPublisher _publisher;

    public PublishBatchHandler(RabbitMqPublisher publisher, ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _publisher = publisher;
    }

    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch != null)
        {
            _logger.LogInformation("No validation errors. Publishing message");
            var json = JsonSerializer.Serialize(context.Batch);
            //publish
        }
    }
}
