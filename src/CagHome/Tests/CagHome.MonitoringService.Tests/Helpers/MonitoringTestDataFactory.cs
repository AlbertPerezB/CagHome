using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Tests.Helpers;

internal static class MonitoringTestDataFactory
{
    internal static BatchReceived CreateBatch() =>
        new(
            BatchId: Guid.NewGuid(),
            PatientId: Guid.NewGuid(),
            Measurements:
            [
                new MeasurementItem(
                    MeasurementId: Guid.NewGuid(),
                    MeasurementType: "HeartRate",
                    Value: 100,
                    Unit: "bpm",
                    DeviceReported: DateTime.UtcNow,
                    ValidationErrors: []
                )
            ],
            ReceivedAtUtc: DateTime.UtcNow
        );

    internal static PolicyDecisionResult CreatePolicyResult(
        BatchReceived batch,
        Careplan careplan,
        Severity? severity,
        bool shouldAlertPatient,
        bool shouldAlertHospital
    ) =>
        new(
            PatientId: batch.PatientId,
            BatchId: batch.BatchId,
            Careplan: careplan,
            Severity: severity,
            ShouldAlertPatient: shouldAlertPatient,
            ShouldAlertHospital: shouldAlertHospital,
            Message: "test message",
            Reasons: [],
            PolicyName: "TestPolicy",
            EvaluatedAtUtc: DateTime.UtcNow
        );
}