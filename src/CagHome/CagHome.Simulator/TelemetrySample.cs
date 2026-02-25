namespace CagHome.Simulator;

public sealed record TelemetrySample(
	DateTimeOffset Timestamp,
	string DeviceId,
	string PatientId,
	string Profile,
	int HeartRateBpm,
	string RhythmFlag,
	double HrvRmssdMs,
	int Spo2Pct,
	double TemperatureC);
