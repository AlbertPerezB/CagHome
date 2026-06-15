using System.Collections.Frozen;
using System.Text;
using System.Text.Json;
using CagHome.MockApplication.Domain.Models;
using CagHome.MockApplication.Domain.Profiles;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace CagHome.MockApplication.Application;

/// <summary>
/// Background service that samples biometric telemetry and publishes patient batches to MQTT.
/// </summary>
/// <param name="logger">Logger used for simulator lifecycle and publish diagnostics.</param>
/// <param name="optionsMonitor">Options source used to retrieve current simulator settings.</param>
/// <param name="profiles">Registered simulation profiles used to generate telemetry samples.</param>
/// <param name="registrationService">The registration service used to register patients in the mock EHR via HTTPS. This value
/// is null unless the RegisterPatients flag is set to true in the appsettings.</param>
public class MockApplicationService(
    ILogger<MockApplicationService> logger,
    IOptionsMonitor<SimulatorOptions> optionsMonitor,
    IEnumerable<ISimulationProfile> profiles,
    PatientRegistrationService? registrationService = null
) : BackgroundService
{
    private readonly Random _random = new();
    private List<Guid> patientIds = new();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<Guid, List<MeasurementPayload>> _accumulatedMeasurementsByPatient =
    [];
    private static readonly MeasurementSourcePayload DefaultMeasurementSource = new(
        "simulator-device",
        "Apple",
        "Watch Series 9"
    );
    private readonly FrozenDictionary<string, ISimulationProfile> _profilesByName =
        profiles.ToFrozenDictionary(profile => profile.Name, StringComparer.OrdinalIgnoreCase);
    private IMqttClient? _mqttClient;
    private bool _notificationTopicSubscribed;
    private readonly Dictionary<Guid, DateTime> _lastPublishTimeByPatient = [];

    // Make sure a default profile called "normal" is registered
    private readonly ISimulationProfile _defaultProfile =
        profiles.FirstOrDefault(profile =>
            profile.Name.Equals(SimulationProfiles.Normal, StringComparison.OrdinalIgnoreCase)
        )
        ?? throw new InvalidOperationException(
            "A default simulation profile named 'normal' must be registered."
        );

    /// <summary>
    /// Runs the simulator worker loop, ensuring MQTT connectivity and publishing telemetry on a fixed interval.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that is triggered when the host is shutting down.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        var options = GetValidatedOptions(optionsMonitor.CurrentValue);

        if (registrationService is not null)
        {
            var registeredIds = await registrationService.RegisterAsync(
                options.DeviceCount,
                stoppingToken
            );
            foreach (var id in registeredIds)
                AddPatientToPool(id, options.PublishBatchIntervalSeconds);
        }
        else
        {
            for (var i = 0; i < options.DeviceCount; i++)
                AddPatientToPool(Guid.NewGuid(), options.PublishBatchIntervalSeconds);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                options = GetValidatedOptions(optionsMonitor.CurrentValue);

                if (options.DeviceCount > patientIds.Count)
                    await GrowPatientPoolAsync(
                        options.DeviceCount,
                        options.PublishBatchIntervalSeconds,
                        stoppingToken
                    );

                var profile = ResolveProfile(options.Profile);
                await EnsureConnectedAsync(options, stoppingToken);
                SampleAndAccumulateBiometrics(options, profile);
                await PublishAccumulatedBatchAsync(options, stoppingToken);

                await Task.Delay(
                    TimeSpan.FromSeconds(options.PublishBiometricsIntervalSeconds),
                    stoppingToken
                );
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
    /// Grows the patient pool to match the target device count by registering new patients if a registration service is available,
    /// or by generating new GUIDs if no registration service is available.
    /// </summary>
    /// <param name="targetCount">The target number of patients in the pool.</param>
    /// <param name="publishBatchIntervalSeconds">The interval in seconds for publishing batches of biometrics.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task GrowPatientPoolAsync(
        int targetCount,
        int publishBatchIntervalSeconds,
        CancellationToken ct
    )
    {
        var toAdd = targetCount - patientIds.Count;
        logger.LogInformation(
            $"DeviceCount increased — adding {toAdd} new patients ({patientIds.Count} -> {targetCount})"
        );

        if (registrationService is not null)
        {
            var newIds = await registrationService.RegisterAsync(toAdd, ct);
            foreach (var id in newIds)
                AddPatientToPool(id, publishBatchIntervalSeconds);
        }
        else
        {
            for (var i = 0; i < toAdd; i++)
                AddPatientToPool(Guid.NewGuid(), publishBatchIntervalSeconds);
        }
    }

    /// <summary>
    /// Adds a patient ID to the pool and initializes their last publish time with a random offset to stagger publish times across patients.
    /// </summary>
    /// <param name="patientId">The ID of the patient to add to the pool.</param>
    /// <param name="publishBatchIntervalSeconds">The interval in seconds for publishing batches of biometrics.</param>
    private void AddPatientToPool(Guid patientId, int publishBatchIntervalSeconds)
    {
        patientIds.Add(patientId);
        var offsetSeconds = _random.Next(0, publishBatchIntervalSeconds);
        _lastPublishTimeByPatient[patientId] =
            DateTime.UtcNow - TimeSpan.FromSeconds(offsetSeconds);
    }

    /// <summary>
    /// Returns configured simulator options after applying value normalization and guardrails.
    /// </summary>
    /// <returns>A validated <see cref="SimulatorOptions"/> instance.</returns>
    public static SimulatorOptions GetValidatedOptions(SimulatorOptions source)
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
            DeviceCount = source.DeviceCount,
            PublishBiometricsIntervalSeconds = Math.Clamp(
                source.PublishBiometricsIntervalSeconds,
                1,
                60
            ),
            PublishBatchIntervalSeconds = Math.Clamp(source.PublishBatchIntervalSeconds, 10, 600),
        };
    }

    /// <summary>
    /// Resolves the active simulation profile strategy for the current publish cycle.
    /// </summary>
    /// <param name="profileName">Configured profile name.</param>
    /// <returns>The resolved profile strategy, falling back to normal when unknown.</returns>
    public ISimulationProfile ResolveProfile(string profileName)
    {
        if (_profilesByName.TryGetValue(profileName, out var profile))
        {
            return profile;
        }

        logger.LogWarning(
            "Unknown simulation profile '{Profile}'. Falling back to '{FallbackProfile}'.",
            profileName,
            _defaultProfile.Name
        );

        return _defaultProfile;
    }

    /// <summary>
    /// Ensures the MQTT client is connected to the configured broker.
    /// </summary>
    /// <param name="options">Resolved simulator options.</param>
    /// <param name="cancellationToken">Token used to cancel the connect operation.</param>
    private async Task EnsureConnectedAsync(
        SimulatorOptions options,
        CancellationToken cancellationToken
    )
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

        logger.LogDebug(
            "Connecting simulator to MQTT broker at {Host}:{Port}",
            options.BrokerHost,
            options.BrokerPort
        );
        await _mqttClient.ConnectAsync(mqttOptions, cancellationToken);

        if (!_notificationTopicSubscribed)
        {
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter("patients/+/notifications")
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);
            _notificationTopicSubscribed = true;
            logger.LogInformation("Simulator subscribed to topic patients/+/notifications");
        }
    }

    /// <summary>
    /// Handles inbound MQTT notifications consumed by the simulated device.
    /// </summary>
    /// <param name="args">Message metadata and payload received from MQTT.</param>
    /// <returns>A completed task.</returns>
    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        logger.LogInformation("Yes hello, patient received the alert");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Samples biometric telemetry for all configured devices and accumulates measurements for batch publishing.
    /// </summary>
    /// <param name="options">Resolved simulator options.</param>
    /// <param name="profile">Active simulation profile strategy.</param>
    private void SampleAndAccumulateBiometrics(SimulatorOptions options, ISimulationProfile profile)
    {
        foreach (var id in patientIds)
        {
            var telemetry = profile.CreateSample(_random);
            var measurements = CreateMeasurements(telemetry);

            if (!_accumulatedMeasurementsByPatient.ContainsKey(id))
                _accumulatedMeasurementsByPatient[id] = new List<MeasurementPayload>();

            _accumulatedMeasurementsByPatient[id].AddRange(measurements);
        }

        logger.LogDebug(
            "Sampled {Count} biometric measurements from profile '{Profile}'",
            options.DeviceCount,
            profile.Name
        );
    }

    /// <summary>
    /// Publishes accumulated measurements as batches for each patient that is due for a publish.
    /// </summary>
    /// <param name="options">Resolved simulator options.</param>
    /// <param name="cancellationToken">Token used to cancel publish operations.</param>
    private async Task PublishAccumulatedBatchAsync(
        SimulatorOptions options,
        CancellationToken cancellationToken
    )
    {
        if (_mqttClient is null || !_mqttClient.IsConnected)
            return;

        var now = DateTime.UtcNow;
        var published = 0;

        foreach (var (patientId, measurements) in _accumulatedMeasurementsByPatient)
        {
            // Check if this specific patient is due for a publish
            if (!_lastPublishTimeByPatient.TryGetValue(patientId, out var lastPublish))
                lastPublish = DateTime.MinValue;

            if (now - lastPublish < TimeSpan.FromSeconds(options.PublishBatchIntervalSeconds))
                continue; // not due yet

            if (measurements.Count == 0)
                continue;

            var accumulatedBatch = new MeasurementBatchPayload(
                SchemaVersion: 1,
                AppVersion: "2.0.0",
                CorrelationId: Guid.NewGuid(),
                PatientId: patientId,
                Measurements: measurements.ToArray()
            );

            var payload = JsonSerializer.Serialize(accumulatedBatch, _jsonOptions);
            var topic = $"{options.TopicPrefix}/{patientId:D}/telemetry";

            await _mqttClient.PublishAsync(
                new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(Encoding.UTF8.GetBytes(payload))
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build(),
                cancellationToken
            );

            _lastPublishTimeByPatient[patientId] = now;
            measurements.Clear(); // clear only this patient's accumulated measurements
            published++;
        }

        if (published > 0)
            logger.LogInformation(
                "Published batches for {Published}/{Total} patients",
                published,
                patientIds.Count
            );
    }

    /// <summary>
    /// Creates normalized measurement payloads from a telemetry sample.
    /// </summary>
    /// <param name="telemetry">Telemetry sample to transform.</param>
    /// <returns>Array of measurement payloads derived from the sample.</returns>
    public static MeasurementPayload[] CreateMeasurements(TelemetrySample telemetry)
    {
        return
        [
            CreateMeasurement("HeartRate", telemetry.HeartRateBpm, "Bpm", telemetry.Timestamp),
            CreateMeasurement("Spo2", telemetry.Spo2Pct, "Percent", telemetry.Timestamp),
            CreateMeasurement("BodyTemperature", telemetry.TemperatureC, "C", telemetry.Timestamp),
        ];
    }

    /// <summary>
    /// Creates a single measurement payload instance.
    /// </summary>
    /// <param name="type">Measurement type name.</param>
    /// <param name="value">Measurement value.</param>
    /// <param name="unit">Measurement unit.</param>
    /// <param name="deviceReported">Device timestamp for the measurement.</param>
    /// <returns>A measurement payload ready for batching.</returns>
    private static MeasurementPayload CreateMeasurement(
        string type,
        double value,
        string unit,
        DateTimeOffset deviceReported
    )
    {
        return new MeasurementPayload(
            MeasurementId: Guid.NewGuid(),
            Type: type,
            Value: value,
            Unit: unit,
            DeviceReported: deviceReported,
            Source: DefaultMeasurementSource
        );
    }

    /// <summary>
    /// Stops the simulator worker and disconnects the MQTT client if connected.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the stop operation.</param>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient is not null)
        {
            _mqttClient.ApplicationMessageReceivedAsync -= OnMessageReceivedAsync;
        }

        if (_mqttClient is { IsConnected: true })
        {
            await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
        }

        _mqttClient?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
