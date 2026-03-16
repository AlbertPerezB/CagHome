using System.Text;
using System.Text.Json;
using System.Collections.Frozen;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace CagHome.Simulator;

public sealed class BiometricPublisherService(
	ILogger<BiometricPublisherService> logger,
	IOptionsMonitor<SimulatorOptions> optionsMonitor,
	IEnumerable<ISimulationProfile> profiles) : BackgroundService
{
	private readonly Random _random = new();
	private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
	private readonly FrozenDictionary<string, ISimulationProfile> _profilesByName =
		profiles.ToFrozenDictionary(profile => profile.Name, StringComparer.OrdinalIgnoreCase);
	private IMqttClient? _mqttClient;
	private readonly ISimulationProfile _defaultProfile = profiles.FirstOrDefault(profile =>
		profile.Name.Equals(SimulationProfiles.Normal, StringComparison.OrdinalIgnoreCase))
		?? throw new InvalidOperationException("A default simulation profile named 'normal' must be registered.");

	/// <summary>
	/// Runs the simulator worker loop, ensuring MQTT connectivity and publishing telemetry on a fixed interval.
	/// </summary>
	/// <param name="stoppingToken">A cancellation token that is triggered when the host is shutting down.</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var factory = new MqttClientFactory();
		_mqttClient = factory.CreateMqttClient();

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var options = GetValidatedOptions(optionsMonitor.CurrentValue);
				var profile = ResolveProfile(options.Profile);

				await EnsureConnectedAsync(options, stoppingToken);
				await PublishBatchAsync(options, profile, stoppingToken);

				var intervalSeconds = options.PublishIntervalSeconds;
				intervalSeconds = Math.Clamp(intervalSeconds, 1, 60);
				await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Simulator loop error. Retrying in 2 seconds.");
				await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
			}
		}
	}

	/// <summary>
	/// Returns configured simulator options after applying value normalization and guardrails.
	/// </summary>
	/// <returns>A validated <see cref="SimulatorOptions"/> instance.</returns>
	private static SimulatorOptions GetValidatedOptions(SimulatorOptions source)
	{
		var profile = string.IsNullOrWhiteSpace(source.Profile)
			? SimulationProfiles.Normal
			: source.Profile.Trim().ToLowerInvariant();

		return new SimulatorOptions
		{
			BrokerHost = source.BrokerHost,
			BrokerPort = source.BrokerPort,
			TopicPrefix = source.TopicPrefix,
			Profile = profile,
			DeviceCount = Math.Clamp(source.DeviceCount, 1, 10),
			PublishIntervalSeconds = Math.Clamp(source.PublishIntervalSeconds, 1, 60),
			DevicePrefix = source.DevicePrefix,
			PatientPrefix = source.PatientPrefix
		};
	}

	/// <summary>
	/// Resolves the active simulation profile strategy for the current publish cycle.
	/// </summary>
	/// <param name="profileName">Configured profile name.</param>
	/// <returns>The resolved profile strategy, falling back to normal when unknown.</returns>
	private ISimulationProfile ResolveProfile(string profileName)
	{
		if (_profilesByName.TryGetValue(profileName, out var profile))
		{
			return profile;
		}

		logger.LogWarning(
			"Unknown simulation profile '{Profile}'. Falling back to '{FallbackProfile}'.",
			profileName,
			_defaultProfile.Name);

		return _defaultProfile;
	}

	/// <summary>
	/// Ensures the MQTT client is connected to the configured broker.
	/// </summary>
	/// <param name="options">Resolved simulator options.</param>
	/// <param name="cancellationToken">Token used to cancel the connect operation.</param>
	private async Task EnsureConnectedAsync(SimulatorOptions options, CancellationToken cancellationToken)
	{
		if (_mqttClient is null || _mqttClient.IsConnected)
		{
			return;
		}

		var mqttOptions = new MqttClientOptionsBuilder()
			.WithClientId($"CagHomeSimulator-{Guid.NewGuid():N}") // N format: 32 digits, no hyphens.
			.WithTcpServer(options.BrokerHost, options.BrokerPort)
			.WithCleanSession()
			.Build();

		logger.LogInformation("Connecting simulator to MQTT broker at {Host}:{Port}", options.BrokerHost, options.BrokerPort);
		await _mqttClient.ConnectAsync(mqttOptions, cancellationToken);
	}

	/// <summary>
	/// Publishes a telemetry message batch for all configured devices.
	/// </summary>
	/// <param name="options">Resolved simulator options.</param>
	/// <param name="profile">Active simulation profile strategy.</param>
	/// <param name="cancellationToken">Token used to cancel publish operations.</param>
	private async Task PublishBatchAsync(SimulatorOptions options, ISimulationProfile profile, CancellationToken cancellationToken)
	{
		if (_mqttClient is null || !_mqttClient.IsConnected)
		{
			return;
		}

		for (var index = 1; index <= options.DeviceCount; index++)
		{
			var telemetry = profile.CreateSample(options, index, _random);
			var payload = JsonSerializer.Serialize(telemetry, _jsonOptions);
			// Topic pattern: biometrics/{deviceId}/telemetry
			var topic = $"{options.TopicPrefix}/{telemetry.DeviceId}/telemetry";

			var message = new MqttApplicationMessageBuilder()
				.WithTopic(topic)
				.WithPayload(Encoding.UTF8.GetBytes(payload))
				.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
				.Build();

			await _mqttClient.PublishAsync(message, cancellationToken);
		}

		logger.LogInformation(
			"Published {Count} telemetry samples with profile '{Profile}'",
			options.DeviceCount,
			profile.Name);
	}

	/// <summary>
	/// Stops the simulator worker and disconnects the MQTT client if connected.
	/// </summary>
	/// <param name="cancellationToken">A token used to cancel the stop operation.</param>
	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_mqttClient is { IsConnected: true })
		{
			await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
		}

		_mqttClient?.Dispose();
		await base.StopAsync(cancellationToken);
	}
}
