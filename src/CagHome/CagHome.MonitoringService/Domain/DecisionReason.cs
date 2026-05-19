namespace CagHome.MonitoringService.Domain;

public record DecisionReason(
    string Metric,
    double ObservedValue,
    string Unit,
    string RuleId,
    string Explanation
);
