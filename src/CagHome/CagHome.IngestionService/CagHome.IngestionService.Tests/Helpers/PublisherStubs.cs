using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure;
using CagHome.IngestionService.Infrastructure.Messaging;

public class RabbitMqPublisherStub : RabbitMqPublisher
{
    public override Task PublishAsync(Batch batch) => Task.CompletedTask;
}

public class MqttPublisherStub : MqttPublisher
{
    public override Task PublishAsync(string topic, string payload) => Task.CompletedTask;
}
