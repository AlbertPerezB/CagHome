namespace CagHome.Simulator;

public sealed record TelemetrySample(
	DateTimeOffset Timestamp,
	int HeartRateBpm,
	string RhythmFlag,
	double HrvRmssdMs,
	int Spo2Pct,
	double TemperatureC);
