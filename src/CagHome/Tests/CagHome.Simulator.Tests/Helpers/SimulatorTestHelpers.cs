using CagHome.Simulator;
using CagHome.Simulator.Application;
using CagHome.Simulator.Domain.Profiles;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CagHome.Tests;

internal static class SimulatorTestHelpers
{
    internal static BiometricPublisherService CreateService()
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

    private class TestOptionsMonitor(SimulatorOptions currentValue)
        : IOptionsMonitor<SimulatorOptions>
    {
        public SimulatorOptions CurrentValue => currentValue;

        public SimulatorOptions Get(string? name) => currentValue;

        public IDisposable? OnChange(Action<SimulatorOptions, string?> listener) => null;
    }
}
