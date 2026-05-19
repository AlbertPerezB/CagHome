using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Tests.Helpers;

internal class TestPolicy(Careplan careplan) : ICareplanDecisionPolicy
{
    public Careplan Careplan { get; } = careplan;

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