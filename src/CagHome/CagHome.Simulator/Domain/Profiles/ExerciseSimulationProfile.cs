using CagHome.Simulator.Domain.Models;

namespace CagHome.Simulator.Domain.Profiles;

/// <summary>
/// Produces telemetry samples representing elevated activity levels.
/// </summary>
public class ExerciseSimulationProfile : ISimulationProfile
{
	/// <summary>
	/// Gets the profile name.
	/// </summary>
	public string Name => SimulationProfiles.Exercise;

	/// <summary>
	/// Creates one exercise telemetry sample.
	/// </summary>
	/// <param name="random">The random number generator used to produce values.</param>
	/// <returns>A telemetry sample in exercise physiological ranges.</returns>
	public TelemetrySample CreateSample(Random random)
	{
		return new TelemetrySample(
			Timestamp: DateTimeOffset.UtcNow,
			HeartRateBpm: NextValue(random, 112, 156),
			RhythmFlag: "normal",
			HrvRmssdMs: NextDouble(random, 12, 30),
			Spo2Pct: NextValue(random, 94, 98),
			TemperatureC: NextDouble(random, 36.8, 37.8));
	}

	/// <summary>
	/// Generates an integer value within an inclusive range.
	/// </summary>
	/// <param name="random">The random number generator used to produce values.</param>
	/// <param name="minInclusive">The minimum allowed value (inclusive).</param>
	/// <param name="maxInclusive">The maximum allowed value (inclusive).</param>
	/// <returns>A random integer between the provided bounds.</returns>
	private static int NextValue(Random random, int minInclusive, int maxInclusive) =>
		random.Next(minInclusive, maxInclusive + 1);

	/// <summary>
	/// Generates a one-decimal-place value within an inclusive range.
	/// </summary>
	/// <param name="random">The random number generator used to produce values.</param>
	/// <param name="minInclusive">The minimum allowed value (inclusive).</param>
	/// <param name="maxInclusive">The maximum allowed value (inclusive).</param>
	/// <returns>A rounded random double between the provided bounds.</returns>
	private static double NextDouble(Random random, double minInclusive, double maxInclusive)
	{
		return Math.Round(minInclusive + (random.NextDouble() * (maxInclusive - minInclusive)), 1);
	}
}