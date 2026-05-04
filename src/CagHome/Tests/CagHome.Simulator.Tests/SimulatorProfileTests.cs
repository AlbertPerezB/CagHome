using CagHome.Simulator.Domain.Profiles;
using Xunit;

namespace CagHome.Tests;

public class SimulatorProfileTests
{
    [Fact]
    public void ProfileNamesMatchConstants()
    {
        Assert.Equal(SimulationProfiles.Normal, new NormalSimulationProfile().Name);
        Assert.Equal(SimulationProfiles.Exercise, new ExerciseSimulationProfile().Name);
        Assert.Equal(SimulationProfiles.Arrhythmia, new ArrhythmiaSimulationProfile().Name);
    }

    [Fact]
    public void NormalProfileSampleStaysWithinExpectedRanges()
    {
        var profile = new NormalSimulationProfile();
        var random = new Random(42);

        for (var i = 0; i < 200; i++)
        {
            var sample = profile.CreateSample(random);

            Assert.Equal("normal", sample.RhythmFlag);
            Assert.InRange(sample.HeartRateBpm, 64, 82);
            Assert.InRange(sample.HrvRmssdMs, 28.0, 55.0);
            Assert.InRange(sample.Spo2Pct, 96, 99);
            Assert.InRange(sample.TemperatureC, 36.4, 37.1);
        }
    }

    [Fact]
    public void ExerciseProfileSampleStaysWithinExpectedRanges()
    {
        var profile = new ExerciseSimulationProfile();
        var random = new Random(73);

        for (var i = 0; i < 200; i++)
        {
            var sample = profile.CreateSample(random);

            Assert.Equal("normal", sample.RhythmFlag);
            Assert.InRange(sample.HeartRateBpm, 112, 156);
            Assert.InRange(sample.HrvRmssdMs, 12.0, 30.0);
            Assert.InRange(sample.Spo2Pct, 94, 98);
            Assert.InRange(sample.TemperatureC, 36.8, 37.8);
        }
    }

    [Fact]
    public void ArrhythmiaProfileSampleMatchesRhythmSpecificRanges()
    {
        var profile = new ArrhythmiaSimulationProfile();
        var random = new Random(101);

        for (var i = 0; i < 400; i++)
        {
            var sample = profile.CreateSample(random);

            Assert.Contains(sample.RhythmFlag, new[] { "normal", "irregular" });
            Assert.InRange(sample.Spo2Pct, 93, 98);
            Assert.InRange(sample.TemperatureC, 36.5, 37.5);

            if (sample.RhythmFlag == "irregular")
            {
                Assert.InRange(sample.HeartRateBpm, 120, 170);
                Assert.InRange(sample.HrvRmssdMs, 5.0, 20.0);
            }
            else
            {
                Assert.InRange(sample.HeartRateBpm, 70, 100);
                Assert.InRange(sample.HrvRmssdMs, 22.0, 45.0);
            }
        }
    }
}
