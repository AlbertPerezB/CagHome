namespace CagHome.Simulator;

public sealed class NormalSimulationProfile : ISimulationProfile
{
	public string Name => SimulationProfiles.Normal;

	public TelemetrySample CreateSample(SimulatorOptions options, int index, Random random)
	{
		return new TelemetrySample(
			Timestamp: DateTimeOffset.UtcNow,
			Profile: SimulationProfiles.Normal,
			HeartRateBpm: NextValue(random, 64, 82),
			RhythmFlag: "normal",
			HrvRmssdMs: NextDouble(random, 28, 55),
			Spo2Pct: NextValue(random, 96, 99),
			TemperatureC: NextDouble(random, 36.4, 37.1));
	}

	private static int NextValue(Random random, int minInclusive, int maxInclusive) =>
		random.Next(minInclusive, maxInclusive + 1);

	private static double NextDouble(Random random, double minInclusive, double maxInclusive)
	{
		return Math.Round(minInclusive + (random.NextDouble() * (maxInclusive - minInclusive)), 1);
	}
}