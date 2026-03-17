namespace CagHome.Simulator;

public sealed class ExerciseSimulationProfile : ISimulationProfile
{
	public string Name => SimulationProfiles.Exercise;

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

	private static int NextValue(Random random, int minInclusive, int maxInclusive) =>
		random.Next(minInclusive, maxInclusive + 1);

	private static double NextDouble(Random random, double minInclusive, double maxInclusive)
	{
		return Math.Round(minInclusive + (random.NextDouble() * (maxInclusive - minInclusive)), 1);
	}
}