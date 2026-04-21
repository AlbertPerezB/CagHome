using CagHome.Contracts;

namespace CagHome.IngestionService.Infrastructure.Messaging
{
    public interface IRabbitMqPublisher
    {
        Task PublishBatchReceived(BatchReceived message);
    }
}
