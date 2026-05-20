using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Application.Decision.Policies;

/// <summary>
/// Provides shared threshold-based evaluation logic for careplan decision policies.
/// </summary>
public abstract class ThresholdCareplanPolicyBase : ICareplanDecisionPolicy
{
    public abstract Careplan Careplan { get; }

    /// <summary>
    /// Gets the metric threshold definitions used by this policy.
    /// </summary>
    protected abstract IReadOnlyList<MetricThreshold> Thresholds { get; }

    /// <summary>
    /// Evaluates a telemetry batch against configured warning and critical thresholds.
    /// </summary>
    /// <param name="context">The batch and careplan context for evaluation.</param>
    /// <returns>The policy decision result from threshold evaluation.</returns>
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

    /// <summary>
    /// Gets measurements from a batch that match the requested metric.
    /// </summary>
    /// <param name="batch">The telemetry batch containing measurements.</param>
    /// <param name="metric">The metric name to filter by.</param>
    /// <returns>A sequence of matching measurements.</returns>
    private static IEnumerable<MeasurementItem> GetMeasurements(BatchReceived batch, string metric)
    {
        return batch.Measurements.Where(m =>
            string.Equals(m.MeasurementType, metric, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Determines whether a value breaches either lower or upper bounds for a threshold band.
    /// </summary>
    /// <param name="value">The measurement value to evaluate.</param>
    /// <param name="band">The threshold band containing lower and upper bounds.</param>
    /// <param name="boundary">When breached, indicates where upper or lower was crossed.</param>
    /// <returns><see langword="true"/> if the value breaches a bound, otherwise, <see langword="false"/>.</returns>
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

    /// <summary>
    /// Determines whether a value breaches the lower bound of a threshold band.
    /// </summary>
    /// <param name="value">The measurement value to evaluate.</param>
    /// <param name="band">The threshold band containing the lower bound definition.</param>
    /// <returns><see langword="true"/> if the lower bound is breached, otherwise, <see langword="false"/>.</returns>
    private static bool IsLowerBreached(double value, ThresholdBand band)
    {
        if (!band.Lower.HasValue)
        {
            return false;
        }

        return band.LowerInclusive ? value <= band.Lower.Value : value < band.Lower.Value;
    }

    /// <summary>
    /// Determines whether a value breaches the upper bound of a threshold band.
    /// </summary>
    /// <param name="value">The measurement value to evaluate.</param>
    /// <param name="band">The threshold band containing the upper bound definition.</param>
    /// <returns><see langword="true"/> if the upper bound is breached, otherwise, <see langword="false"/>.</returns>
    private static bool IsUpperBreached(double value, ThresholdBand band)
    {
        if (!band.Upper.HasValue)
        {
            return false;
        }

        return band.UpperInclusive ? value >= band.Upper.Value : value > band.Upper.Value;
    }

    /// <summary>
    /// Creates a decision reason describing a breached threshold.
    /// </summary>
    /// <param name="threshold">The metric threshold definition that was evaluated.</param>
    /// <param name="measurement">The measurement that breached the threshold.</param>
    /// <param name="level">The severity level label associated with the breach.</param>
    /// <param name="boundary">The breached boundary label.</param>
    /// <param name="explanation">An explanation of the breach.</param>
    /// <returns>A decision reason instance.</returns>
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

    /// <summary>
    /// Defines warning and critical threshold bands for a specific metric.
    /// </summary>
    /// <param name="Metric">The metric identifier to evaluate.</param>
    /// <param name="Warning">The warning threshold band.</param>
    /// <param name="Critical">The critical threshold band.</param>
    protected record MetricThreshold(
        string Metric,
        ThresholdBand Warning,
        ThresholdBand Critical
    );

    /// <summary>
    /// Defines lower and upper bounds for threshold comparison.
    /// </summary>
    /// <param name="Lower">The optional lower bound.</param>
    /// <param name="LowerInclusive">Indicates whether lower-bound comparison is inclusive.</param>
    /// <param name="Upper">The optional upper bound.</param>
    /// <param name="UpperInclusive">Indicates whether upper-bound comparison is inclusive.</param>
    protected record ThresholdBand(
        double? Lower = null,
        bool LowerInclusive = false,
        double? Upper = null,
        bool UpperInclusive = false
    );
}
