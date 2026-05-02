namespace CagHome.MonitoringService.Domain;

public sealed record DecisionReason(
    string Metric,
    double ObservedValue,
    string Unit,
    string RuleId,
    string Explanation
);
