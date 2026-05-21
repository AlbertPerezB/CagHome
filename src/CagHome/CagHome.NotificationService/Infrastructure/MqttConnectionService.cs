using MQTTnet;

namespace CagHome.NotificationService.Infrastructure;

/// <summary>
/// Provides a background service that manages a persistent connection to an MQTT broker using configurable connection
/// options.
/// </summary>
/// <remarks>This service establishes and maintains an MQTT connection, automatically attempting to reconnect if
/// the connection is lost. The connection parameters are read from configuration settings ("MQTT_HOST" and "MQTT_PORT").
///</remarks>
public class MqttConnectionService : BackgroundService, IAsyncDisposable
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly ILogger<MqttConnectionService> _logger;

    public IMqttClient Client => _client;

    /// <summary>
    /// Initializes a new instance of the MqttConnectionService class using the specified configuration and logger.
    /// </summary>
    /// <param name="configuration">The configuration provider used to retrieve MQTT connection settings such as host and port.</param>
    /// <param name="logger">The logger used to record diagnostic and operational messages for the MQTT connection service.</param>
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
                _logger.LogDebug("MQTT reconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT reconnection failed");
            }
        };
    }

    /// <summary>
    /// Executes the background service operation to establish a connection to the MQTT broker.
    /// </summary>
    /// <remarks>This method is called by the host to run the background service. It attempts to connect to
    /// the MQTT broker and completes when the connection is established or the operation is canceled.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous execution operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Connecting to MQTT broker...");
        await _client.ConnectAsync(_options, cancellationToken);
        _logger.LogDebug("MQTT connected");
    }

    /// <summary>
    /// Asynchronously disconnects the MQTT client if it is currently connected.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the disconnect operation.</param>
    /// <returns>A task that represents the asynchronous disconnect operation.</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
            _logger.LogDebug("MQTT disconnected cleanly");
        }
    }

    /// <summary>
    /// Asynchronously releases all resources used by the current instance and disconnects the client if it is
    /// connected.
    /// </summary>
    /// <remarks>Call this method to ensure that all network connections are properly closed and resources are
    /// released when the instance is no longer needed. This method should be called instead of Dispose when
    /// asynchronous cleanup is required.</remarks>
    /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
        }
        _client.Dispose();
    }
}
