namespace CagHome.MockApplication;

public class SimulatorOptions
{
    public const string SectionName = "Simulator";

    public required string BrokerHost { get; set; }
    public int BrokerPort { get; set; }
    public int DeviceCount { get; set; }
    public required string Profile { get; set; }
    public int PublishBatchIntervalSeconds { get; set; }
    public int PublishBiometricsIntervalSeconds { get; set; }
    public required string TopicPrefix { get; set; }
    public bool RegisterPatients { get; set; } = false;

    public List<Guid> PatientIds { get; set; } = new List<Guid>();
}
