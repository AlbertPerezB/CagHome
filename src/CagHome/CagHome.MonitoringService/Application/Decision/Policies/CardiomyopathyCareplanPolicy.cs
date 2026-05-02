using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Application.Decision.Policies;

public sealed class CardiomyopathyCareplanPolicy : ThresholdCareplanPolicyBase
{
    public override Careplan Careplan => Careplan.Cardiomyopathy;

    protected override IReadOnlyList<MetricThreshold> Thresholds =>
    [
        new(
            "HeartRate",
            Warning: new ThresholdBand(Lower: 50, Upper: 100),
            Critical: new ThresholdBand(Lower: 40, Upper: 115)
        ),
        new(
            "Spo2",
            Warning: new ThresholdBand(Lower: 94),
            Critical: new ThresholdBand(Lower: 91)
        ),
        new(
            "BodyTemperature",
            Warning: new ThresholdBand(Upper: 38.0, UpperInclusive: true),
            Critical: new ThresholdBand(Lower: 35.0, Upper: 39.0, UpperInclusive: true)
        )
    ];
}
