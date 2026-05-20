using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Domain;

/// <summary>
/// Represents the outcome produced directly by policy evaluation.
/// </summary>
/// <param name="PatientId">The unique id of the patient being evaluated.</param>
/// <param name="BatchId">The unique id of the telemetry batch.</param>
/// <param name="Careplan">The careplan used during evaluation.</param>
/// <param name="Severity">The severity determined by policy evaluation, if any.</param>
/// <param name="ShouldAlertPatient">Indicates whether a patient alert should be emitted.</param>
/// <param name="ShouldAlertHospital">Indicates whether a hospital alert should be emitted.</param>
/// <param name="Message">Message describing the decision.</param>
/// <param name="Reasons">The collection of reasons supporting the decision.</param>
/// <param name="PolicyName">The name of the policy that produced this decision.</param>
/// <param name="EvaluatedAtUtc">The UTC timestamp when policy evaluation completed.</param>
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
