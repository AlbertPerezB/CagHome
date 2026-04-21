using System.Text;
using System.Text.Json;
using MQTTnet;

namespace CagHome.NotificationService.Infrastructure;

public class MqttNotificationPublisher(
    MqttConnectionService connectionService,
    ILogger<MqttNotificationPublisher> logger
)
{
    public async Task Publish(Guid patientId, object payload)
    {
        var client = connectionService.Client;

        if (!client.IsConnected)
        {
            throw new InvalidOperationException("MQTT client is not connected");
        }

        var topic = $"patients/{patientId}/notifications";
        var json = JsonSerializer.Serialize(payload);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(json))
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        await client.PublishAsync(message);

        logger.LogInformation("Published to {Topic}", topic);
    }
}
