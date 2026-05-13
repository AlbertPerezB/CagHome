using System.Text;
using System.Text.Json;
using CagHome.Simulator.Domain.Models;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace CagHome.Simulator.Application;

public interface IInjectedTelemetryPublisher
{
    Task PublishAsync(
        MeasurementBatchPayload batchPayload,
        Guid patientId,
        CancellationToken cancellationToken
    );
}

public sealed class InjectedTelemetryPublisher(
    IOptionsMonitor<SimulatorOptions> optionsMonitor,
    ILogger<InjectedTelemetryPublisher> logger
) : IInjectedTelemetryPublisher, IAsyncDisposable
{
    private const string TopicPrefix = "biometrics";
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IMqttClient _mqttClient = new MqttClientFactory().CreateMqttClient();
    private readonly SemaphoreSlim _connectionGate = new(1, 1);

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