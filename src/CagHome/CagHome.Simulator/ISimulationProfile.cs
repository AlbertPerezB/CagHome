namespace CagHome.Simulator;

public interface ISimulationProfile
{
	string Name { get; }

	TelemetrySample CreateSample(SimulatorOptions options, int index, Random random);
}