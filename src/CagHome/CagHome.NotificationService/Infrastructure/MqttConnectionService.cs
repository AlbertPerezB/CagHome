using MQTTnet;

namespace CagHome.NotificationService.Infrastructure;

public class MqttConnectionService : IHostedService, IAsyncDisposable
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly ILogger<MqttConnectionService> _logger;

    public IMqttClient Client => _client;

    public MqttConnectionService(
        IConfiguration configuration,
        ILogger<MqttConnectionService> logger
    )
    {
        _logger = logger;
        _client = new MqttClientFactory().CreateMqttClient();

        var host = configuration["MQTT_HOST"] ?? "localhost";
        var port = int.Parse(configuration["MQTT_PORT"] ?? "1883");

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId($"notification-service-{Guid.NewGuid():N}")
            .WithCleanSession(false)
            .Build();

        _client.DisconnectedAsync += async args =>
        {
            _logger.LogWarning("MQTT disconnected, reconnecting in 5s...");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await _client.ConnectAsync(_options);
                _logger.LogInformation("MQTT reconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT reconnection failed");
            }
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Connecting to MQTT broker...");
        await _client.ConnectAsync(_options, cancellationToken);
        _logger.LogInformation("MQTT connected");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
            _logger.LogInformation("MQTT disconnected cleanly");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
        }
        _client.Dispose();
    }
}
