using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Tests.Helpers;

/// <summary>
/// Creates common test data used by MonitoringService tests.
/// </summary>
internal static class MonitoringTestDataFactory
{
    /// <summary>
    /// Creates a batch with generated values and a default heart rate measurement.
    /// </summary>
    /// <returns>A populated <see cref="BatchReceived"/> test instance.</returns>
    internal static BatchReceived CreateBatch() =>
        new(
            BatchId: Guid.NewGuid(),
            CorrelationId: Guid.NewGuid(),
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

    /// <summary>
    /// Creates a policy decision result aligned with the supplied batch and alert flags.
    /// </summary>
    /// <param name="batch">The source batch used for patient and batch identifiers.</param>
    /// <param name="careplan">The careplan used in the decision result.</param>
    /// <param name="severity">The severity to apply to the decision.</param>
    /// <param name="shouldAlertPatient">Whether the decision should alert the patient.</param>
    /// <param name="shouldAlertHospital">Whether the decision should alert the hospital.</param>
    /// <returns>A populated <see cref="PolicyDecisionResult"/>.</returns>
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

    /// <summary>
    /// Creates a batch for a specific patient and heart rate value.
    /// </summary>
    /// <param name="patientId">The patient identifier to include in the batch.</param>
    /// <param name="heartRate">The heart rate measurement value in bpm.</param>
    /// <returns>A populated <see cref="BatchReceived"/> test instance.</returns>
    internal static BatchReceived CreateBatch(Guid patientId, double heartRate) =>
        new(
            BatchId: Guid.NewGuid(),
            CorrelationId: Guid.NewGuid(),
            PatientId: patientId,
            Measurements:
            [
                new MeasurementItem(
                    MeasurementId: Guid.NewGuid(),
                    MeasurementType: "HeartRate",
                    Value: heartRate,
                    Unit: "bpm",
                    DeviceReported: DateTime.UtcNow,
                    ValidationErrors: []
                )
            ],
            ReceivedAtUtc: DateTime.UtcNow
        );
}