using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Server;

namespace CagHome.MqttBroker;

/// <summary>
/// Hosts and manages the MQTT broker lifecycle for the application.
/// </summary>
public class MqttBrokerService : IHostedService
{
    private readonly ILogger<MqttBrokerService> _logger;
    private MqttServer? _mqttServer;
    private readonly int _port;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttBrokerService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to write broker lifecycle and client activity events.</param>
    public MqttBrokerService(ILogger<MqttBrokerService> logger)
    {
        _logger = logger;
        _port = int.TryParse(Environment.GetEnvironmentVariable("MQTT_PORT"), out var port)
            ? port
            : 1883;
    }

    /// <summary>
    /// Starts the MQTT broker and registers broker event handlers.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the startup operation.</param>
    /// <returns>A task that completes when the broker has started.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting MQTT Broker on port {Port}", _port);

        var mqttFactory = new MqttServerFactory();

        var mqttServerOptions = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(_port)
            .Build();

        _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);

        // Subscribe to client connection events for logging
        _mqttServer.ClientConnectedAsync += OnClientConnectedAsync;
        _mqttServer.ClientDisconnectedAsync += OnClientDisconnectedAsync;
        _mqttServer.ClientSubscribedTopicAsync += OnClientSubscribedTopicAsync;
        _mqttServer.ClientUnsubscribedTopicAsync += OnClientUnsubscribedTopicAsync;

        await _mqttServer.StartAsync();

        _logger.LogDebug("MQTT Broker started successfully on port {Port}", _port);
    }

    /// <summary>
    /// Stops and disposes the MQTT broker instance.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the shutdown operation.</param>
    /// <returns>A task that completes when the broker has stopped.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopping MQTT Broker");

        if (_mqttServer != null)
        {
            await _mqttServer.StopAsync();
            _mqttServer.Dispose();
        }

        _logger.LogDebug("MQTT Broker stopped");
    }

    /// <summary>
    /// Handles connection events and logs the client details.
    /// </summary>
    /// <param name="eventArgs">Connection event from the MQTT client.</param>
    /// <returns>A completed task.</returns>
    private Task OnClientConnectedAsync(ClientConnectedEventArgs eventArgs)
    {
        _logger.LogDebug(
            "Client connected: {ClientId} With {UserName}",
            eventArgs.ClientId,
            eventArgs.UserName
        );
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles disconnection events and logs the information.
    /// </summary>
    /// <param name="eventArgs">Disconnection event from the MQTT client.</param>
    /// <returns>A completed task.</returns>
    private Task OnClientDisconnectedAsync(ClientDisconnectedEventArgs eventArgs)
    {
        _logger.LogDebug(
            "Client disconnected: {ClientId}, Type: {DisconnectType}",
            eventArgs.ClientId,
            eventArgs.DisconnectType
        );
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles topic subscription events and logs the subscribed topic.
    /// </summary>
    /// <param name="eventArgs">Subscription event from the MQTT client.</param>
    /// <returns>A completed task.</returns>
    private Task OnClientSubscribedTopicAsync(ClientSubscribedTopicEventArgs eventArgs)
    {
        _logger.LogDebug(
            "Client {ClientId} subscribed to topic: {Topic}",
            eventArgs.ClientId,
            eventArgs.TopicFilter.Topic
        );
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles topic unsubscription events and logs the unsubscribed topic.
    /// </summary>
    /// <param name="eventArgs">Unsubscription event from the MQTT client.</param>
    /// <returns>A completed task.</returns>
    private Task OnClientUnsubscribedTopicAsync(ClientUnsubscribedTopicEventArgs eventArgs)
    {
        _logger.LogDebug(
            "Client {ClientId} unsubscribed from topic: {Topic}",
            eventArgs.ClientId,
            eventArgs.TopicFilter
        );
        return Task.CompletedTask;
    }
}
