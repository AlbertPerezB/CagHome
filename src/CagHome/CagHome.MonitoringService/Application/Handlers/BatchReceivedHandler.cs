using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Domain;
using CagHome.MonitoringService.Infrastructure;
using Wolverine;

namespace CagHome.MonitoringService.Application.Handlers;

/// <summary>
/// Handles incoming telemetry batches by evaluating policy outcomes, publishing alerts, and auditing decisions.
/// </summary>
/// <param name="patientCareplanStore">Store used to retrieve the current careplan for a patient.</param>
/// <param name="policyResolver">Resolver used to select the evaluation policy for a careplan.</param>
/// <param name="cooldownService">Service used to enforce alert cooldown windows.</param>
/// <param name="decisionAuditStore">Store used to save decision audit records.</param>
/// <param name="logger">Logger for evaluation and publishing events.</param>
public class BatchReceivedHandler(
    IPatientCareplanStore patientCareplanStore,
    ICareplanPolicyResolver policyResolver,
    ICooldownService cooldownService,
    IDecisionAuditStore decisionAuditStore,
    ILogger<BatchReceivedHandler> logger)
{
    /// <summary>
    /// Processes a received telemetry batch and publishes resulting alerts when applicable.
    /// </summary>
    /// <param name="message">The telemetry batch message to evaluate.</param>
    /// <param name="messageBus">The message bus used to publish alert requests.</param>
    /// <returns>A task when evaluation, publication, and auditing are finished.</returns>
    public async Task Handle(BatchReceived message, IMessageBus messageBus)
    {
        var careplan = await patientCareplanStore.TryGet(message.PatientId) ?? Careplan.None;
        var context = new BatchEvaluationContext(message, careplan);

        var policy = policyResolver.Resolve(careplan);
        var policyResult = policy.Evaluate(context);

        var patientAlertPublished = false;
        var hospitalAlertPublished = false;
        var suppressedByCooldown = false;
        TimeSpan? remainingCooldown = null;

        if (policyResult.Severity is Severity severity)
        {
            var cooldownResult = cooldownService.Evaluate(
                message.PatientId,
                severity,
                DateTime.UtcNow
            );

            suppressedByCooldown = cooldownResult.IsSuppressed;
            remainingCooldown = cooldownResult.RemainingCooldown;

            if (!suppressedByCooldown)
            {
                var alertId = Guid.NewGuid();

                if (policyResult.ShouldAlertPatient)
                {
                    await messageBus.PublishAsync(
                        new PatientAlertRequested(
                            PatientId: message.PatientId,
                            Message: policyResult.Message,
                            Severity: severity,
                            DecidedAt: DateTime.UtcNow,
                            CorrelationId: message.CorrelationId,
                            AlertId: alertId
                        )
                    );
                    patientAlertPublished = true;
                }

                if (policyResult.ShouldAlertHospital)
                {
                    // TODO: Replace Guid.Empty with mapped HospitalId once patient registry includes it in Monitoring context.
                    await messageBus.PublishAsync(
                        new HospitalAlertRequested(
                            PatientId: message.PatientId,
                            HospitalId: Guid.Empty,
                            Message: policyResult.Message,
                            Severity: severity,
                            DecidedAt: DateTime.UtcNow,
                            CorrelationId: message.CorrelationId,
                            AlertId: alertId
                        )
                    );
                    hospitalAlertPublished = true;
                }
            }
        }

        var auditEntry = new DecisionAuditEntry
        {
            PatientId = message.PatientId,
            BatchId = message.BatchId,
            CorrelationId = message.CorrelationId,
            Careplan = careplan,
            PolicyName = policyResult.PolicyName,
            Severity = policyResult.Severity,
            ShouldAlertPatient = policyResult.ShouldAlertPatient,
            ShouldAlertHospital = policyResult.ShouldAlertHospital,
            SuppressedByCooldown = suppressedByCooldown,
            RemainingCooldown = remainingCooldown,
            PatientAlertPublished = patientAlertPublished,
            HospitalAlertPublished = hospitalAlertPublished,
            Message = policyResult.Message,
            Reasons = policyResult.Reasons,
            TimestampUtc = DateTime.UtcNow,
        };

        await decisionAuditStore.RecordAuditEntry(auditEntry);

        logger.LogInformation(
            "Batch evaluated: BatchId={BatchId}, PatientId={PatientId}, Careplan={Careplan}, Severity={Severity}, SuppressedByCooldown={SuppressedByCooldown}",
            message.BatchId,
            message.PatientId,
            careplan,
            policyResult.Severity,
            suppressedByCooldown
        );
    }
}
