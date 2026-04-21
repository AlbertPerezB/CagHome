using System.Text.Json;
using CagHome.Contracts;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Messaging;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class PublishBatchHandler(IRabbitMqPublisher publisher, ILogger<PublishBatchHandler> logger)
    : IngestionHandler
{
    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch != null)
        {
            var batch = context.Batch;
            await publisher.PublishBatchReceived(GetBatchReceived(context.Batch));
        }
    }

    private BatchReceived GetBatchReceived(Batch b) =>
        new BatchReceived(
            b.BatchId,
            b.PatientId,
            b.Measurements.Select(GetMeasurementItem).ToList(),
            b.ReceivedAt
        );

    private MeasurementItem GetMeasurementItem(Measurement m) =>
        new MeasurementItem(
            m.MeasurementId,
            m.MeasurementType.ToString(),
            m.Value,
            m.Unit.ToString(),
            m.DeviceReported,
            m.ValidationErrors.Select(GetValidationErrorItem).ToList()
        );

    private ValidationErrorItem GetValidationErrorItem(ValidationError e) =>
        new ValidationErrorItem(e.Message, e.Code.ToString());
}
