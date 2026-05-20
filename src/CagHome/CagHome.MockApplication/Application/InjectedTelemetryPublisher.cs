using System.Text;
using System.Text.Json;
using CagHome.MockApplication.Domain.Models;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using CagHome.MockApplication.Infrastructure;


namespace CagHome.MockApplication.Application;

/// <summary>
/// MQTT implementation of <see cref="IInjectedTelemetryPublisher"/> for publishing injected telemetry batches.
/// </summary>
/// <param name="optionsMonitor">Options monitor that provides MQTT broker configuration.</param>
/// <param name="logger">Logger used for publish and connection lifecycle events.</param>
public class InjectedTelemetryPublisher(
    IOptionsMonitor<SimulatorOptions> optionsMonitor,
    ILogger<InjectedTelemetryPublisher> logger
) : IInjectedTelemetryPublisher, IAsyncDisposable
{
    private const string TopicPrefix = "biometrics";
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IMqttClient _mqttClient = new MqttClientFactory().CreateMqttClient();
    private readonly SemaphoreSlim _connectionGate = new(1, 1);

    /// <summary>
    /// Publishes a telemetry batch payload for a patient-specific topic.
    /// </summary>
    /// <param name="batchPayload">Payload to serialize and publish.</param>
    /// <param name="patientId">Identifier of the patient used in the topic path.</param>
    /// <param name="cancellationToken">Token that can cancel connect or publish operations.</param>
    /// <returns>A task when the message is published.</returns>
    public async Task PublishAsync(
        MeasurementBatchPayload batchPayload,
        Guid patientId,
        CancellationToken cancellationToken
    )
    {
        await EnsureConnectedAsync(cancellationToken);

        var topic = $"{TopicPrefix}/{patientId:D}/telemetry";
        var payload = JsonSerializer.Serialize(batchPayload, _jsonOptions);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(payload))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message, cancellationToken);

        logger.LogInformation(
            "Published injected telemetry to topic {Topic}",
            topic
        );
    }

    /// <summary>
    /// Ensures an active MQTT connection exists before publishing.
    /// </summary>
    /// <param name="cancellationToken">Token that can cancel the connection process.</param>
    /// <returns>A task when the client is connected.</returns>
    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient.IsConnected)
        {
            return;
        }

        await _connectionGate.WaitAsync(cancellationToken);
        try
        {
            if (_mqttClient.IsConnected)
            {
                return;
            }

            var options = optionsMonitor.CurrentValue;
            if (string.IsNullOrWhiteSpace(options.BrokerHost))
            {
                throw new InvalidOperationException("Simulator:BrokerHost must be configured.");
            }

            if (options.BrokerPort <= 0)
            {
                throw new InvalidOperationException("Simulator:BrokerPort must be greater than 0.");
            }

            var mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId($"CagHomeSimulatorInject-{Guid.NewGuid():N}")
                .WithTcpServer(options.BrokerHost, options.BrokerPort)
                .WithCleanSession()
                .Build();

            await _mqttClient.ConnectAsync(mqttOptions, cancellationToken);
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    /// <summary>
    /// Disconnects and disposes MQTT resources used by this publisher.
    /// </summary>
    /// <returns>A value task when cleanup finishes.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.DisconnectAsync();
        }

        _mqttClient.Dispose();
        _connectionGate.Dispose();
    }
}