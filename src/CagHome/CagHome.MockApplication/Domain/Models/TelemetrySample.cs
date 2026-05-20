namespace CagHome.MockApplication.Domain.Models;

/// <summary>
/// Generated telemetry produced by the simulator before payload mapping.
/// </summary>
/// <param name="HeartRateBpm">Heart rate in beats per minute.</param>
/// <param name="HrvRmssdMs">Heart rate variability (RMSSD) in milliseconds.</param>
/// <param name="RhythmFlag">Rhythm classification flag, such as normal or arrhythmia.</param>
/// <param name="Spo2Pct">Peripheral oxygen saturation percentage.</param>
/// <param name="TemperatureC">Body temperature in degrees Celsius.</param>
/// <param name="Timestamp">UTC timestamp indicating when the sample was generated.</param>
public record TelemetrySample(
	int HeartRateBpm,
	double HrvRmssdMs,
	string RhythmFlag,
	int Spo2Pct,
	double TemperatureC,
	DateTimeOffset Timestamp);
