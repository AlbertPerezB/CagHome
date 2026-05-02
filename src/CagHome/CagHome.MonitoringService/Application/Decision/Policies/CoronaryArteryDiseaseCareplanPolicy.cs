using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Application.Decision.Policies;

public sealed class CoronaryArteryDiseaseCareplanPolicy : ThresholdCareplanPolicyBase
{
    public override Careplan Careplan => Careplan.CoronaryArteryDisease;

    protected override IReadOnlyList<MetricThreshold> Thresholds =>
    [
        new(
            "HeartRate",
            Warning: new ThresholdBand(Lower: 50, Upper: 100),
            Critical: new ThresholdBand(Lower: 40, Upper: 120)
        ),
        new(
            "Spo2",
            Warning: new ThresholdBand(Lower: 94),
            Critical: new ThresholdBand(Lower: 92)
        ),
        new(
            "BodyTemperature",
            Warning: new ThresholdBand(Upper: 38.0, UpperInclusive: true),
            Critical: new ThresholdBand(Lower: 35.0, Upper: 39.0, UpperInclusive: true)
        )
    ];
}
