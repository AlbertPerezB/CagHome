namespace CagHome.Simulator.Domain.Models;

public record TelemetrySample(
	int HeartRateBpm,
	double HrvRmssdMs,
	string RhythmFlag,
	int Spo2Pct,
	double TemperatureC,
	DateTimeOffset Timestamp);
