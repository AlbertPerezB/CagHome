using CagHome.Simulator.Application;
using CagHome.Simulator.Domain.Models;
using CagHome.Simulator.Domain.Profiles;
using CagHome.Simulator.Tests.Helpers;
using Xunit;

namespace CagHome.Simulator.Tests.UnitTests;

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

        var validated = BiometricPublisherService.GetValidatedOptions(source);

        Assert.Equal("exercise", validated.Profile);
        Assert.Equal(10, validated.DeviceCount);
        Assert.Equal(1, validated.PublishBiometricsIntervalSeconds);
        Assert.Equal(10, validated.PublishBatchIntervalSeconds);
    }

    [Fact]
    public void ResolveProfile_UnknownNameFallsBackToNormal()
    {
        var service = SimulatorTestHelpers.CreateService();

        var resolved = service.ResolveProfile("this-does-not-exist");

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

        var measurements = BiometricPublisherService.CreateMeasurements(telemetry);

        Assert.Equal(3, measurements.Length);
        Assert.Equal(
            ["HeartRate", "Spo2", "BodyTemperature"],
            measurements.Select(m => m.Type)
        );
        Assert.Equal(77, measurements.Single(m => m.Type == "HeartRate").Value);
        Assert.Equal(97, measurements.Single(m => m.Type == "Spo2").Value);
        Assert.Equal(37.2, measurements.Single(m => m.Type == "BodyTemperature").Value);
    }
}
