namespace CagHome.MonitoringService.Domain;

/// <summary>
/// Represents a reason contributing to a policy decision.
/// </summary>
/// <param name="Metric">The metric name evaluated by the policy.</param>
/// <param name="ObservedValue">The observed metric value from telemetry.</param>
/// <param name="Unit">The unit associated with <paramref name="ObservedValue"/>.</param>
/// <param name="RuleId">The id of the policy rule that matched.</param>
/// <param name="Explanation">An explanation of the matched rule.</param>
public record DecisionReason(
    string Metric,
    double ObservedValue,
    string Unit,
    string RuleId,
    string Explanation
);
