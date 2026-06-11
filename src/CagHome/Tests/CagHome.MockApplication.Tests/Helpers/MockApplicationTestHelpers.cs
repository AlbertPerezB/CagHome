using CagHome.MockApplication.Application;
using CagHome.MockApplication.Domain.Profiles;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CagHome.MockApplication.Tests.Helpers;

/// <summary>
/// Provides test-only factory methods and lightweight infrastructure for creating simulator services.
/// </summary>
internal static class MockApplicationTestHelpers
{
    /// <summary>
    /// Creates a <see cref="MockApplication"/> instance configured with deterministic test defaults.
    /// </summary>
    /// <returns>
    /// A configured <see cref="MockApplication"/> that uses in-memory options and known simulation profiles.
    /// </returns>
    internal static MockApplicationService CreateService()
    {
        var options = new SimulatorOptions
        {
            BrokerHost = "localhost",
            BrokerPort = 1883,
            TopicPrefix = "biometrics",
            Profile = SimulationProfiles.Normal,
            SampleBiometricsIntervalSeconds = 2,
            PublishBatchIntervalSeconds = 60,
            PatientIds = [Guid.Parse("11111111-1111-1111-1111-111111111111")],
        };

        var profiles = new ISimulationProfile[]
        {
            new NormalSimulationProfile(),
            new ExerciseSimulationProfile(),
            new ArrhythmiaSimulationProfile(),
        };

        return new MockApplicationService(
            NullLogger<MockApplicationService>.Instance,
            new TestOptionsMonitor(options),
            profiles
        );
    }

    /// <summary>
    /// Minimal <see cref="IOptionsMonitor{TOptions}"/> implementation for tests.
    /// </summary>
    /// <param name="currentValue">The options value returned by this monitor.</param>
    private class TestOptionsMonitor(SimulatorOptions currentValue)
        : IOptionsMonitor<SimulatorOptions>
    {
        /// <summary>
        /// Gets the current options value.
        /// </summary>
        public SimulatorOptions CurrentValue => currentValue;

        /// <summary>
        /// Gets the options value for the supplied name.
        /// </summary>
        /// <param name="name">The named options instance to retrieve. Ignored in this test implementation.</param>
        /// <returns>The configured test options value.</returns>
        public SimulatorOptions Get(string? name) => currentValue;

        /// <summary>
        /// Registers a change listener.
        /// </summary>
        /// <param name="listener">The callback invoked when options change.</param>
        /// <returns>
        /// <see langword="null"/> because this test implementation does not publish change notifications.
        /// </returns>
        public IDisposable? OnChange(Action<SimulatorOptions, string?> listener) => null;
    }
}
