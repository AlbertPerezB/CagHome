using System.Reflection;
using CagHome.Simulator;
using CagHome.Simulator.Application;
using CagHome.Simulator.Domain.Models;
using CagHome.Simulator.Domain.Profiles;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CagHome.Tests;

public class SimulatorUnitTests
{
    [Fact]
    public void GetValidatedOptions_ClampsAndNormalizesValues()
    {
        var source = new SimulatorOptions
        {
            BrokerHost = "localhost",
            BrokerPort = 1883,
            TopicPrefix = "biometrics",
            Profile = " EXERCISE ",
            DeviceCount = 99,
            PublishBiometricsIntervalSeconds = 0,
            PublishBatchIntervalSeconds = 1,
        };

        var validated = InvokePrivateStatic<SimulatorOptions>("GetValidatedOptions", source);

        Assert.Equal("exercise", validated.Profile);
        Assert.Equal(10, validated.DeviceCount);
        Assert.Equal(1, validated.PublishBiometricsIntervalSeconds);
        Assert.Equal(10, validated.PublishBatchIntervalSeconds);
    }

    [Fact]
    public void ResolveProfile_UnknownNameFallsBackToNormal()
    {
        var service = CreateService();

        var resolved = InvokePrivateInstance<ISimulationProfile>(
            service,
            "ResolveProfile",
            "this-does-not-exist"
        );

        Assert.Equal(SimulationProfiles.Normal, resolved.Name);
    }

    [Fact]
    public void CreateMeasurements_BuildsExpectedThreeMeasurements()
    {
        var telemetry = new TelemetrySample(
            Timestamp: DateTimeOffset.UtcNow,
            HeartRateBpm: 77,
            RhythmFlag: "irregular",
            HrvRmssdMs: 13.5,
            Spo2Pct: 97,
            TemperatureC: 37.2
        );

        var measurements = InvokePrivateStatic<MeasurementPayload[]>(
            "CreateMeasurements",
            telemetry
        );

        Assert.Equal(3, measurements.Length);
        Assert.Equal(
            new[] { "HeartRate", "Spo2", "BodyTemperature" },
            measurements.Select(m => m.Type)
        );
        Assert.Equal(77, measurements.Single(m => m.Type == "HeartRate").Value);
        Assert.Equal(97, measurements.Single(m => m.Type == "Spo2").Value);
        Assert.Equal(37.2, measurements.Single(m => m.Type == "BodyTemperature").Value);
    }

    private static BiometricPublisherService CreateService()
    {
        var options = new SimulatorOptions
        {
            BrokerHost = "localhost",
            BrokerPort = 1883,
            TopicPrefix = "biometrics",
            Profile = SimulationProfiles.Normal,
            DeviceCount = 1,
            PublishBiometricsIntervalSeconds = 2,
            PublishBatchIntervalSeconds = 60,
        };

        var profiles = new ISimulationProfile[]
        {
            new NormalSimulationProfile(),
            new ExerciseSimulationProfile(),
            new ArrhythmiaSimulationProfile(),
        };

        return new BiometricPublisherService(
            NullLogger<BiometricPublisherService>.Instance,
            new TestOptionsMonitor(options),
            profiles
        );
    }

    private static T InvokePrivateStatic<T>(string methodName, params object[] args)
    {
        var method =
            typeof(BiometricPublisherService).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static
            )
            ?? throw new InvalidOperationException(
                $"Expected static method '{methodName}' was not found."
            );

        return (T)(
            method.Invoke(null, args)
            ?? throw new InvalidOperationException($"Method '{methodName}' returned null.")
        );
    }

    private static T InvokePrivateInstance<T>(
        object instance,
        string methodName,
        params object[] args
    )
    {
        var method =
            instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException(
                $"Expected instance method '{methodName}' was not found."
            );

        return (T)(
            method.Invoke(instance, args)
            ?? throw new InvalidOperationException($"Method '{methodName}' returned null.")
        );
    }

    private sealed class TestOptionsMonitor(SimulatorOptions currentValue)
        : IOptionsMonitor<SimulatorOptions>
    {
        public SimulatorOptions CurrentValue => currentValue;

        public SimulatorOptions Get(string? name) => currentValue;

        public IDisposable? OnChange(Action<SimulatorOptions, string?> listener) => null;
    }
}
