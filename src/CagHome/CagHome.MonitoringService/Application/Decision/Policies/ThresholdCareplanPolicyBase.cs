using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Application.Decision.Policies;

public abstract class ThresholdCareplanPolicyBase : ICareplanDecisionPolicy
{
    public abstract Careplan Careplan { get; }

    protected abstract IReadOnlyList<MetricThreshold> Thresholds { get; }

    public PolicyDecisionResult Evaluate(BatchEvaluationContext context)
    {
        var criticalReasons = new List<DecisionReason>();
        var warningReasons = new List<DecisionReason>();

        foreach (var threshold in Thresholds)
        {
            foreach (var measurement in GetMeasurements(context.Batch, threshold.Metric))
            {
                if (IsBreached(measurement.Value, threshold.Critical, out var criticalBoundary))
                {
                    criticalReasons.Add(
                        CreateReason(
                            threshold,
                            measurement,
                            "Critical",
                            criticalBoundary,
                            "Critical threshold breached"
                        )
                    );
                    continue;
                }

                if (IsBreached(measurement.Value, threshold.Warning, out var warningBoundary))
                {
                    warningReasons.Add(
                        CreateReason(
                            threshold,
                            measurement,
                            "Warning",
                            warningBoundary,
                            "Warning threshold breached"
                        )
                    );
                }
            }
        }

        Severity? severity = criticalReasons.Count > 0
            ? Severity.Critical
            : warningReasons.Count > 0
                ? Severity.Warning
                : null;

        var reasons = severity switch
        {
            Severity.Critical => criticalReasons,
            Severity.Warning => warningReasons,
            _ => []
        };

        var message = severity is null
            ? $"{GetType().Name}: no thresholds breached."
            : $"{GetType().Name}: {reasons.Count} {severity} rule(s) triggered.";

        return new PolicyDecisionResult(
            PatientId: context.Batch.PatientId,
            BatchId: context.Batch.BatchId,
            Careplan: context.Careplan,
            Severity: severity,
            ShouldAlertPatient: severity is not null,
            ShouldAlertHospital: severity == Severity.Critical,
            Message: message,
            Reasons: reasons,
            PolicyName: GetType().Name,
            EvaluatedAtUtc: DateTime.UtcNow
        );
    }

    private static IEnumerable<MeasurementItem> GetMeasurements(BatchReceived batch, string metric)
    {
        return batch.Measurements.Where(m =>
            string.Equals(m.MeasurementType, metric, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static bool IsBreached(double value, ThresholdBand band, out string boundary)
    {
        if (IsLowerBreached(value, band))
        {
            boundary = "Lower";
            return true;
        }

        if (IsUpperBreached(value, band))
        {
            boundary = "Upper";
            return true;
        }

        boundary = string.Empty;
        return false;
    }

    private static bool IsLowerBreached(double value, ThresholdBand band)
    {
        if (!band.Lower.HasValue)
        {
            return false;
        }

        return band.LowerInclusive ? value <= band.Lower.Value : value < band.Lower.Value;
    }

    private static bool IsUpperBreached(double value, ThresholdBand band)
    {
        if (!band.Upper.HasValue)
        {
            return false;
        }

        return band.UpperInclusive ? value >= band.Upper.Value : value > band.Upper.Value;
    }

    private static DecisionReason CreateReason(
        MetricThreshold threshold,
        MeasurementItem measurement,
        string level,
        string boundary,
        string explanation
    )
    {
        return new DecisionReason(
            Metric: threshold.Metric,
            ObservedValue: measurement.Value,
            Unit: measurement.Unit,
            RuleId: $"{threshold.Metric}.{level}.{boundary}",
            Explanation: explanation
        );
    }

    protected sealed record MetricThreshold(
        string Metric,
        ThresholdBand Warning,
        ThresholdBand Critical
    );

    protected sealed record ThresholdBand(
        double? Lower = null,
        bool LowerInclusive = false,
        double? Upper = null,
        bool UpperInclusive = false
    );
}
