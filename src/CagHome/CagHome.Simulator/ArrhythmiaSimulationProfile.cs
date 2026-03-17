namespace CagHome.Simulator;

public sealed class ArrhythmiaSimulationProfile : ISimulationProfile
{
	public string Name => SimulationProfiles.Arrhythmia;

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

	private static int NextValue(Random random, int minInclusive, int maxInclusive) =>
		random.Next(minInclusive, maxInclusive + 1);

	private static double NextDouble(Random random, double minInclusive, double maxInclusive)
	{
		return Math.Round(minInclusive + (random.NextDouble() * (maxInclusive - minInclusive)), 1);
	}
}