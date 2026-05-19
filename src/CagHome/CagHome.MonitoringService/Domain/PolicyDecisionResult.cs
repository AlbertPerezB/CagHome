using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Domain;

public record PolicyDecisionResult(
    Guid PatientId,
    Guid BatchId,
    Careplan Careplan,
    Severity? Severity,
    bool ShouldAlertPatient,
    bool ShouldAlertHospital,
    string Message,
    IReadOnlyList<DecisionReason> Reasons,
    string PolicyName,
    DateTime EvaluatedAtUtc
);
