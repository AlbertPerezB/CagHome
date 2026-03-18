namespace CagHome.Simulator;

public sealed class SimulatorOptions
{
	public const string SectionName = "Simulator";

	public required string BrokerHost { get; set; }
	public int BrokerPort { get; set; }
	public required string TopicPrefix { get; set; }
	public required string Profile { get; set; }
	public int DeviceCount { get; set; }
	public int PublishIntervalSeconds { get; set; }

}
