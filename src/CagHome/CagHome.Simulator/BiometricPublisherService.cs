using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace CagHome.Simulator;

public sealed class BiometricPublisherService(
	ILogger<BiometricPublisherService> logger,
	IOptions<SimulatorOptions> optionsAccessor) : BackgroundService
{
	private readonly Random _random = new();
	private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
	private readonly SimulatorOptions _options = optionsAccessor.Value;
	private IMqttClient? _mqttClient;

	/// <summary>
	/// Runs the simulator worker loop, ensuring MQTT connectivity and publishing telemetry on a fixed interval.
	/// </summary>
	/// <param name="stoppingToken">A cancellation token that is triggered when the host is shutting down.</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var factory = new MqttClientFactory();
		_mqttClient = factory.CreateMqttClient();
		var options = GetValidatedOptions();

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await EnsureConnectedAsync(options, stoppingToken);
				await PublishBatchAsync(options, options.Profile, stoppingToken);

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
	private SimulatorOptions GetValidatedOptions()
	{
		var options = _options;

		options.DeviceCount = Math.Clamp(options.DeviceCount, 1, 10);
		options.PublishIntervalSeconds = Math.Clamp(options.PublishIntervalSeconds, 1, 60);
		if (!SimulationProfiles.IsSupported(options.Profile))
		{
			options.Profile = SimulationProfiles.Normal;
		}
		else
		{
			options.Profile = options.Profile.ToLowerInvariant();
		}

		return options;
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
	/// <param name="profile">Active simulation profile.</param>
	/// <param name="cancellationToken">Token used to cancel publish operations.</param>
	private async Task PublishBatchAsync(SimulatorOptions options, string profile, CancellationToken cancellationToken)
	{
		if (_mqttClient is null || !_mqttClient.IsConnected)
		{
			return;
		}

		for (var index = 1; index <= options.DeviceCount; index++)
		{
			var telemetry = CreateTelemetry(options, profile, index);
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
			profile);
	}

	/// <summary>
	/// Creates a telemetry sample using the profile-specific value generator.
	/// </summary>
	/// <param name="options">Resolved simulator options.</param>
	/// <param name="profile">Requested simulation profile.</param>
	/// <param name="index">Device index.</param>
	/// <returns>A populated <see cref="TelemetrySample"/>.</returns>
	private TelemetrySample CreateTelemetry(SimulatorOptions options, string profile, int index)
	{
		var normalizedProfile = SimulationProfiles.IsSupported(profile)
			? profile.ToLowerInvariant()
			: SimulationProfiles.Normal;

		return normalizedProfile switch
		{
			SimulationProfiles.Exercise => CreateExerciseSample(options, index),
			SimulationProfiles.Arrhythmia => CreateArrhythmiaSample(options, index),
			_ => CreateNormalSample(options, index)
		};
	}

	/// <summary>
	/// Creates a normal-profile telemetry sample.
	/// </summary>
	/// <param name="options">Resolved simulator options.</param>
	/// <param name="index">Device index.</param>
	/// <returns>A normal-profile telemetry sample.</returns>
	private TelemetrySample CreateNormalSample(SimulatorOptions options, int index)
	{
		return new TelemetrySample(
			Timestamp: DateTimeOffset.UtcNow,
			DeviceId: $"{options.DevicePrefix}-{index:000}",
			PatientId: $"{options.PatientPrefix}-{index:000}",
			Profile: SimulationProfiles.Normal,
			HeartRateBpm: NextValue(64, 82),
			RhythmFlag: "normal",
			HrvRmssdMs: NextDouble(28, 55),
			Spo2Pct: NextValue(96, 99),
			TemperatureC: NextDouble(36.4, 37.1));
	}

	/// <summary>
	/// Creates an exercise-profile telemetry sample.
	/// </summary>
	/// <param name="options">Resolved simulator options.</param>
	/// <param name="index">Device index.</param>
	/// <returns>An exercise-profile telemetry sample.</returns>
	private TelemetrySample CreateExerciseSample(SimulatorOptions options, int index)
	{
		return new TelemetrySample(
			Timestamp: DateTimeOffset.UtcNow,
			DeviceId: $"{options.DevicePrefix}-{index:000}",
			PatientId: $"{options.PatientPrefix}-{index:000}",
			Profile: SimulationProfiles.Exercise,
			HeartRateBpm: NextValue(112, 156),
			RhythmFlag: "normal",
			HrvRmssdMs: NextDouble(12, 30),
			Spo2Pct: NextValue(94, 98),
			TemperatureC: NextDouble(36.8, 37.8));
	}

	/// <summary>
	/// Creates an arrhythmia-profile telemetry sample with periodic irregular rhythm spikes.
	/// </summary>
	/// <param name="options">Resolved simulator options.</param>
	/// <param name="index">Device index.</param>
	/// <returns>An arrhythmia-profile telemetry sample.</returns>
	private TelemetrySample CreateArrhythmiaSample(SimulatorOptions options, int index)
	{
		// 35% of samples are generated as irregular rhythm events.
		var irregular = _random.NextDouble() < 0.35;
		var heartRate = irregular ? NextValue(120, 170) : NextValue(70, 100);
		var hrv = irregular ? NextDouble(5, 20) : NextDouble(22, 45);

		return new TelemetrySample(
			Timestamp: DateTimeOffset.UtcNow,
			DeviceId: $"{options.DevicePrefix}-{index:000}",
			PatientId: $"{options.PatientPrefix}-{index:000}",
			Profile: SimulationProfiles.Arrhythmia,
			HeartRateBpm: heartRate,
			RhythmFlag: irregular ? "irregular" : "normal",
			HrvRmssdMs: hrv,
			Spo2Pct: NextValue(93, 98),
			TemperatureC: NextDouble(36.5, 37.5));
	}

	/// <summary>
	/// Returns a random integer within an inclusive range.
	/// </summary>
	/// <param name="minInclusive">Lower bound (inclusive).</param>
	/// <param name="maxInclusive">Upper bound (inclusive).</param>
	/// <returns>A pseudo-random integer within the specified range.</returns>
	private int NextValue(int minInclusive, int maxInclusive) => _random.Next(minInclusive, maxInclusive + 1);

	/// <summary>
	/// Returns a random floating-point value within an inclusive range, rounded to one decimal place.
	/// </summary>
	/// <param name="minInclusive">Lower bound (inclusive).</param>
	/// <param name="maxInclusive">Upper bound (inclusive).</param>
	/// <returns>A pseudo-random rounded value within the specified range.</returns>
	private double NextDouble(double minInclusive, double maxInclusive)
	{
		return Math.Round(minInclusive + (_random.NextDouble() * (maxInclusive - minInclusive)), 1);
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
