using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Application.Decision.Policies;

public sealed class NoneCareplanPolicy : ThresholdCareplanPolicyBase
{
    public override Careplan Careplan => Careplan.None;

    protected override IReadOnlyList<MetricThreshold> Thresholds =>
    [
        // Heart rate in bpm
        new(
            "HeartRate",
            Warning: new ThresholdBand(Lower: 50, Upper: 110),
            Critical: new ThresholdBand(Lower: 40, Upper: 130)
        ),
        // SpO2 in percent
        new(
            "Spo2",
            Warning: new ThresholdBand(Lower: 93),
            Critical: new ThresholdBand(Lower: 90)
        ),
        // Body temperature in Celsius
        new(
            "BodyTemperature",
            Warning: new ThresholdBand(Upper: 38.0, UpperInclusive: true),
            Critical: new ThresholdBand(Lower: 35.0, Upper: 39.0, UpperInclusive: true)
        )
    ];
}
