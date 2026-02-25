namespace CagHome.Simulator;

public sealed class SimulatorOptions
{
	public const string SectionName = "Simulator";

	public string BrokerHost { get; set; } = "localhost";
	public int BrokerPort { get; set; } = 1883;
	public string TopicPrefix { get; set; } = "biometrics";
	public string Profile { get; set; } = SimulationProfiles.Normal;
	public int DeviceCount { get; set; } = 5;
	public int PublishIntervalSeconds { get; set; } = 2;
	public string DevicePrefix { get; set; } = "wearable";
	public string PatientPrefix { get; set; } = "patient";
}
