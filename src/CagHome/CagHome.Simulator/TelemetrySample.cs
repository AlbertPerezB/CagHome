namespace CagHome.Simulator;

public sealed record TelemetrySample(
	DateTimeOffset Timestamp,
	string Profile,
	int HeartRateBpm,
	string RhythmFlag,
	double HrvRmssdMs,
	int Spo2Pct,
	double TemperatureC);
