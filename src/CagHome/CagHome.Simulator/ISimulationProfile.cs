namespace CagHome.Simulator;

public interface ISimulationProfile
{
	string Name { get; }

	TelemetrySample CreateSample(Random random);
}