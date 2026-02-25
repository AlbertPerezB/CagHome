namespace CagHome.Simulator;

public static class SimulationProfiles
{
	public const string Normal = "normal";
	public const string Exercise = "exercise";
	public const string Arrhythmia = "arrhythmia";

	/// <summary>
	/// Determines whether the provided profile name is supported by the simulator.
	/// </summary>
	/// <param name="profile">Profile name to validate.</param>
	/// <returns><see langword="true"/> when the profile is supported; otherwise, <see langword="false"/>.</returns>
	public static bool IsSupported(string profile) =>
		profile.Equals(Normal, StringComparison.OrdinalIgnoreCase)
		|| profile.Equals(Exercise, StringComparison.OrdinalIgnoreCase)
		|| profile.Equals(Arrhythmia, StringComparison.OrdinalIgnoreCase);
}
