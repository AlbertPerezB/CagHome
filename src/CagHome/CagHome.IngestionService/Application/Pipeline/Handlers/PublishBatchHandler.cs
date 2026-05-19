using CagHome.Contracts;
using CagHome.IngestionService.Domain.Models;
using Wolverine;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class PublishBatchHandler(IMessageBus messageBus, ILogger<PublishBatchHandler> logger)
    : IngestionHandler
{
    protected override async Task ProcessAsync(IngestionContext context)
    {
        if (context.Batch != null)
        {
            var batch = context.Batch;
            await messageBus.PublishAsync(GetBatchReceived(context.Batch));
            logger.LogInformation($"Batch {batch.BatchId} validated and published");
        }
    }

    private BatchReceived GetBatchReceived(Batch b) =>
        new BatchReceived(
            b.BatchId,
            b.CorrelationId,
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
