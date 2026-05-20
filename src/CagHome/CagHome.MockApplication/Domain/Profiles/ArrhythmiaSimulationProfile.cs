namespace CagHome.MockApplication.Domain.Profiles;

using CagHome.MockApplication.Domain.Models;

/// <summary>
/// Produces telemetry samples that (sometimes) simulate arrhythmia events.
/// </summary>
public class ArrhythmiaSimulationProfile : ISimulationProfile
{
	/// <summary>
	/// Gets the profile name.
	/// </summary>
	public string Name => SimulationProfiles.Arrhythmia;

	/// <summary>
	/// Creates one arrhythmia telemetry sample.
	/// </summary>
	/// <param name="random">The random number generator used to produce values.</param>
	/// <returns>A telemetry sample with either normal or irregular rhythm characteristics.</returns>
	public TelemetrySample CreateSample(Random random)
	{
		// 35% of samples are generated as irregular rhythm events.
		var irregular = random.NextDouble() < 0.35;
		var heartRate = irregular ? NextValue(random, 120, 170) : NextValue(random, 70, 100);
		var hrv = irregular ? NextDouble(random, 5, 20) : NextDouble(random, 22, 45);

		return new TelemetrySample(
			Timestamp: DateTimeOffset.UtcNow,
			HeartRateBpm: heartRate,
			RhythmFlag: irregular ? "irregular" : "normal",
			HrvRmssdMs: hrv,
			Spo2Pct: NextValue(random, 93, 98),
			TemperatureC: NextDouble(random, 36.5, 37.5));
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