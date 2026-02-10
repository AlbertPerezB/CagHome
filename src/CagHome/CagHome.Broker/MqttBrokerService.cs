using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;

namespace CagHome.Broker;

public class MqttBrokerService : IHostedService
{
    private readonly ILogger<MqttBrokerService> _logger;
    private MqttServer? _mqttServer;
    private readonly int _port;

    public MqttBrokerService(ILogger<MqttBrokerService> logger)
    {
        _logger = logger;
        _port = int.TryParse(Environment.GetEnvironmentVariable("MQTT_PORT"), out var port) ? port : 1883;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MQTT Broker on port {Port}", _port);

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

        _logger.LogInformation("MQTT Broker started successfully on port {Port}", _port);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MQTT Broker");

        if (_mqttServer != null)
        {
            await _mqttServer.StopAsync();
            _mqttServer.Dispose();
        }

        _logger.LogInformation("MQTT Broker stopped");
    }

    private Task OnClientConnectedAsync(ClientConnectedEventArgs eventArgs)
    {
        _logger.LogInformation("Client connected: {ClientId} With {UserName}", 
            eventArgs.ClientId, 
            eventArgs.UserName);
        return Task.CompletedTask;
    }

    private Task OnClientDisconnectedAsync(ClientDisconnectedEventArgs eventArgs)
    {
        _logger.LogInformation("Client disconnected: {ClientId}, Type: {DisconnectType}", 
            eventArgs.ClientId, 
            eventArgs.DisconnectType);
        return Task.CompletedTask;
    }

    private Task OnClientSubscribedTopicAsync(ClientSubscribedTopicEventArgs eventArgs)
    {
        _logger.LogInformation("Client {ClientId} subscribed to topic: {Topic}", 
            eventArgs.ClientId, 
            eventArgs.TopicFilter.Topic);
        return Task.CompletedTask;
    }

    private Task OnClientUnsubscribedTopicAsync(ClientUnsubscribedTopicEventArgs eventArgs)
    {
        _logger.LogInformation("Client {ClientId} unsubscribed from topic: {Topic}", 
            eventArgs.ClientId, 
            eventArgs.TopicFilter);
        return Task.CompletedTask;
    }

    private async Task PublishEvent()
    {
        if(_mqttServer == null) return;
         var message = new MqttApplicationMessageBuilder().WithTopic("HelloWorld").WithPayload("Test").Build();
         await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message));
    }
}
