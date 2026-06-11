using CagHome.MockApplication.Domain.Models;
using CagHome.MockApplication.Domain.Profiles;
using CagHome.MockApplication.Tests.Helpers;
using CagHome.MockApplication.Application;
using Xunit;

namespace CagHome.MockApplication.Tests.UnitTests;

public class MockApplicationUnitTests
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
            SampleBiometricsIntervalSeconds = 0,
            PublishBatchIntervalSeconds = 1,
            PatientIds =
            [
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ],
        };

        var validated = MockApplicationService.GetValidatedOptions(source);

        Assert.Equal("exercise", validated.Profile);
        Assert.Equal(1, validated.SampleBiometricsIntervalSeconds);
        Assert.Equal(10, validated.PublishBatchIntervalSeconds);
        Assert.Single(validated.PatientIds);
        Assert.Equal(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            validated.PatientIds[0]
        );
    }

    [Fact]
    public void ResolveProfile_UnknownNameFallsBackToNormal()
    {
        var service = MockApplicationTestHelpers.CreateService();

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

        var measurements = MockApplicationService.CreateMeasurements(telemetry);

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
