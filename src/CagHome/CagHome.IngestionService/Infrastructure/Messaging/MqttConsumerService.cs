using System.Text;
using CagHome.IngestionService.Domain.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace CagHome.IngestionService.Infrastructure
{
    public class MqttConsumerService : IHostedService, IDisposable
    {
        private readonly ILogger<MqttConsumerService> _logger;
        private readonly IMqttClient _mqttClient;
        private readonly string _brokerHost;
        private readonly int _brokerPort;
        private readonly string _clientId;
        private readonly CancellationTokenSource _reconnectCts;
        private Task? _reconnectTask;

        public MqttConsumerService(ILogger<MqttConsumerService> logger)
        {
            _logger = logger;
            _reconnectCts = new CancellationTokenSource();

            // Read configuration from environment variables
            _brokerHost = Environment.GetEnvironmentVariable("MQTT_BROKER_HOST") ?? "localhost";
            _brokerPort = int.TryParse(
                Environment.GetEnvironmentVariable("MQTT_BROKER_PORT"),
                out var port
            )
                ? port
                : 1883;

            var instanceId =
                Environment.GetEnvironmentVariable("MQTT_CONSUMER_INSTANCE")
                ?? Guid.NewGuid().ToString();
            _clientId = $"CagHomeConsumer-{instanceId}";

            var mqttFactory = new MqttClientFactory();
            _mqttClient = mqttFactory.CreateMqttClient();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MQTT Consumer: {ClientId}", _clientId);

            // Setup event handlers
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            _mqttClient.ConnectedAsync += OnConnectedAsync;

            await ConnectAsync(cancellationToken);
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_brokerHost, _brokerPort)
                    .WithClientId(_clientId)
                    .WithCleanSession(false)
                    .Build();

                _logger.LogInformation(
                    "Connecting to MQTT Broker at {Host}:{Port}",
                    _brokerHost,
                    _brokerPort
                );

                await _mqttClient.ConnectAsync(options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT Broker");
                throw;
            }
        }

        private async Task OnConnectedAsync(MqttClientConnectedEventArgs args)
        {
            _logger.LogInformation("Connected to MQTT Broker successfully");

            // Subscribe to all topics using wildcard
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter("#")
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions);

            _logger.LogInformation("Subscribed to all topics (#)");
        }

        private async Task<Task> OnMessageReceivedAsync(
            MqttApplicationMessageReceivedEventArgs args
        )
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            var qos = args.ApplicationMessage.QualityOfServiceLevel;
            var retain = args.ApplicationMessage.Retain;

            _logger.LogInformation(
                "Message received - Topic: {Topic}, Payload: {Payload}, QoS: {QoS}, Retain: {Retain}",
                topic,
                payload,
                qos,
                retain
            );
            var rawBatch = new RawBatch(topic, payload, DateTime.UtcNow);
            return Task.CompletedTask;
        }

        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
        {
            _logger.LogWarning("Disconnected from MQTT Broker. Reason: {Reason}", args.Reason);

            // Start reconnection logic
            if (!_reconnectCts.Token.IsCancellationRequested)
            {
                _reconnectTask = ReconnectAsync(_reconnectCts.Token);
            }

            return Task.CompletedTask;
        }

        private async Task ReconnectAsync(CancellationToken cancellationToken)
        {
            var reconnectDelay = TimeSpan.FromSeconds(5);
            var attempt = 1;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation(
                        "Attempting to reconnect to MQTT Broker (Attempt {Attempt})...",
                        attempt
                    );

                    await Task.Delay(reconnectDelay, cancellationToken);
                    await ConnectAsync(cancellationToken);

                    _logger.LogInformation("Reconnected successfully");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Reconnection attempt {Attempt} failed", attempt);
                    attempt++;

                    // Exponential backoff, max 60 seconds
                    reconnectDelay = TimeSpan.FromSeconds(
                        Math.Min(reconnectDelay.TotalSeconds * 2, 60)
                    );
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT Consumer");

            _reconnectCts.Cancel();

            if (_reconnectTask != null)
            {
                await _reconnectTask;
            }

            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
            }

            _logger.LogInformation("MQTT Consumer stopped");
        }

        public void Dispose()
        {
            _mqttClient?.Dispose();
            _reconnectCts?.Dispose();
        }
    }
}
