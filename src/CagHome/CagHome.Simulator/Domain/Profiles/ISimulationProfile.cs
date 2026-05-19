using CagHome.Simulator.Domain.Models;

namespace CagHome.Simulator.Domain.Profiles;

/// <summary>
/// Defines a generation profile used by the simulator.
/// </summary>
public interface ISimulationProfile
{
	/// <summary>
	/// Gets the profile name.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Creates a single telemetry sample using the provided random source.
	/// </summary>
	/// <param name="random">The random number generator used to produce sample values.</param>
	/// <returns>A generated telemetry sample.</returns>
	TelemetrySample CreateSample(Random random);
}