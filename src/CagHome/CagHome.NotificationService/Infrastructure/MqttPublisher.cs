using System.Text;
using System.Text.Json;
using MQTTnet;

namespace CagHome.NotificationService.Infrastructure;

/// <summary>
/// Provides functionality to publish messages to MQTT topics for patient notifications.
/// </summary>
/// <param name="connectionService">The service used to manage and provide access to the MQTT client connection.</param>
/// <param name="logger">The logger used to record publishing operations and diagnostic information.</param>
public class MqttPublisher(MqttConnectionService connectionService, ILogger<MqttPublisher> logger)
    : IMqttPublisher
{
    /// <summary>
    /// Publishes a notification message for the specified patient to the MQTT broker.
    /// </summary>
    /// <remarks>The message is published to the topic 'patients/{patientId}/notifications' with exactly-once
    /// delivery semantics. The payload is serialized as UTF-8 encoded JSON.</remarks>
    /// <param name="patientId">The unique identifier of the patient for whom the notification is being published.</param>
    /// <param name="payload">The notification payload to publish. The object will be serialized to JSON before sending.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the MQTT client is not connected.</exception>
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
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
            .WithRetainFlag(false)
            .Build();

        await client.PublishAsync(message);

        logger.LogInformation($"Published message {json} to {topic}");
    }
}
