using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Tests.Helpers;

/// <summary>
/// Test implementation of <see cref="ICareplanDecisionPolicy"/> that always returns a non-alerting decision.
/// </summary>
/// <param name="careplan">The careplan associated with this policy instance.</param>
internal class TestPolicy(Careplan careplan) : ICareplanDecisionPolicy
{
    /// <summary>
    /// Gets the careplan represented by this policy.
    /// </summary>
    public Careplan Careplan { get; } = careplan;

    /// <summary>
    /// Evaluates a batch context and returns a decision result for tests.
    /// </summary>
    /// <param name="context">The batch evaluation context.</param>
    /// <returns>A <see cref="PolicyDecisionResult"/> with no patient or hospital alerts.</returns>
    public PolicyDecisionResult Evaluate(BatchEvaluationContext context) =>
        new(
            PatientId: context.Batch.PatientId,
            BatchId: context.Batch.BatchId,
            Careplan: context.Careplan,
            Severity: null,
            ShouldAlertPatient: false,
            ShouldAlertHospital: false,
            Message: string.Empty,
            Reasons: [],
            PolicyName: nameof(TestPolicy),
            EvaluatedAtUtc: DateTime.UtcNow
        );
}