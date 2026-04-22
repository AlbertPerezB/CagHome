using CagHome.Simulator.Domain.Models;

namespace CagHome.Simulator.Domain.Profiles;

public interface ISimulationProfile
{
	string Name { get; }

	TelemetrySample CreateSample(Random random);
}