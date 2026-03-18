using CagHome.Simulator;

namespace CagHome.Tests;

public class SimulatorProfileTests
{
    [Test]
    public void ProfileNamesMatchConstants()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new NormalSimulationProfile().Name, Is.EqualTo(SimulationProfiles.Normal));
            Assert.That(new ExerciseSimulationProfile().Name, Is.EqualTo(SimulationProfiles.Exercise));
            Assert.That(new ArrhythmiaSimulationProfile().Name, Is.EqualTo(SimulationProfiles.Arrhythmia));
        });
    }

    [Test]
    public void NormalProfileSampleStaysWithinExpectedRanges()
    {
        var profile = new NormalSimulationProfile();
        var random = new Random(42);

        for (var i = 0; i < 200; i++)
        {
            var sample = profile.CreateSample(random);

            Assert.Multiple(() =>
            {
                Assert.That(sample.RhythmFlag, Is.EqualTo("normal"));
                Assert.That(sample.HeartRateBpm, Is.InRange(64, 82));
                Assert.That(sample.HrvRmssdMs, Is.InRange(28.0, 55.0));
                Assert.That(sample.Spo2Pct, Is.InRange(96, 99));
                Assert.That(sample.TemperatureC, Is.InRange(36.4, 37.1));
            });
        }
    }

    [Test]
    public void ExerciseProfileSampleStaysWithinExpectedRanges()
    {
        var profile = new ExerciseSimulationProfile();
        var random = new Random(73);

        for (var i = 0; i < 200; i++)
        {
            var sample = profile.CreateSample(random);

            Assert.Multiple(() =>
            {
                Assert.That(sample.RhythmFlag, Is.EqualTo("normal"));
                Assert.That(sample.HeartRateBpm, Is.InRange(112, 156));
                Assert.That(sample.HrvRmssdMs, Is.InRange(12.0, 30.0));
                Assert.That(sample.Spo2Pct, Is.InRange(94, 98));
                Assert.That(sample.TemperatureC, Is.InRange(36.8, 37.8));
            });
        }
    }

    [Test]
    public void ArrhythmiaProfileSampleMatchesRhythmSpecificRanges()
    {
        var profile = new ArrhythmiaSimulationProfile();
        var random = new Random(101);

        for (var i = 0; i < 400; i++)
        {
            var sample = profile.CreateSample(random);

            Assert.Multiple(() =>
            {
                Assert.That(sample.RhythmFlag, Is.EqualTo("normal").Or.EqualTo("irregular"));
                Assert.That(sample.Spo2Pct, Is.InRange(93, 98));
                Assert.That(sample.TemperatureC, Is.InRange(36.5, 37.5));
            });

            if (sample.RhythmFlag == "irregular")
            {
                Assert.That(sample.HeartRateBpm, Is.InRange(120, 170));
                Assert.That(sample.HrvRmssdMs, Is.InRange(5.0, 20.0));
            }
            else
            {
                Assert.That(sample.HeartRateBpm, Is.InRange(70, 100));
                Assert.That(sample.HrvRmssdMs, Is.InRange(22.0, 45.0));
            }
        }
    }
}
