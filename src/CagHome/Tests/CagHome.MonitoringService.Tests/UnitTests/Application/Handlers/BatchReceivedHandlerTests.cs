using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Application.Handlers;
using CagHome.MonitoringService.Domain;
using CagHome.MonitoringService.Infrastructure;
using CagHome.MonitoringService.Tests.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Wolverine;

namespace CagHome.MonitoringService.Tests.Application.Handlers;

public class BatchReceivedHandlerTests
{
    private readonly IPatientCareplanStore _patientCareplanStore;
    private readonly ICareplanPolicyResolver _policyResolver;
    private readonly ICooldownService _cooldownService;
    private readonly IDecisionAuditStore _decisionAuditStore;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<BatchReceivedHandler> _logger;
    private readonly ICareplanDecisionPolicy _policy;
    private readonly BatchReceivedHandler _handler;

    public BatchReceivedHandlerTests()
    {
        _patientCareplanStore = Substitute.For<IPatientCareplanStore>();
        _policyResolver = Substitute.For<ICareplanPolicyResolver>();
        _cooldownService = Substitute.For<ICooldownService>();
        _decisionAuditStore = Substitute.For<IDecisionAuditStore>();
        _messageBus = Substitute.For<IMessageBus>();
        _logger = Substitute.For<ILogger<BatchReceivedHandler>>();
        _policy = Substitute.For<ICareplanDecisionPolicy>();
        _handler = new BatchReceivedHandler(_patientCareplanStore, _policyResolver, _cooldownService, _decisionAuditStore, _logger);
    }

    [Fact]
    public async Task Handle_WithWarningAndNoCooldown_PublishesPatientAlertAndAudits()
    {
        var message = MonitoringTestDataFactory.CreateBatch();
        _patientCareplanStore.TryGet(message.PatientId).Returns(Careplan.Cardiomyopathy);
        _policyResolver.Resolve(Careplan.Cardiomyopathy).Returns(_policy);

        _policy.Evaluate(Arg.Any<BatchEvaluationContext>())
            .Returns(
                MonitoringTestDataFactory.CreatePolicyResult(
                    message,
                    Careplan.Cardiomyopathy,
                    Severity.Warning,
                    true,
                    false
                )
            );

        _cooldownService.Evaluate(message.PatientId, Severity.Warning, Arg.Any<DateTime>())
            .Returns(new CooldownCheckResult(IsSuppressed: false, RemainingCooldown: null));

        await _handler.Handle(message, _messageBus);

        await _messageBus
            .Received(1)
            .PublishAsync(Arg.Is<PatientAlertRequested>(m => m.PatientId == message.PatientId));
        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<HospitalAlertRequested>());

        await _decisionAuditStore
            .Received(1)
            .RecordAuditEntry(
                Arg.Is<DecisionAuditEntry>(e =>
                    e.PatientId == message.PatientId
                    && e.BatchId == message.BatchId
                    && e.CorrelationId == message.CorrelationId
                    && e.Severity == Severity.Warning
                    && e.SuppressedByCooldown == false
                    && e.PatientAlertPublished
                    && !e.HospitalAlertPublished
                )
            );
    }

    [Fact]
    public async Task Handle_WithCriticalSuppressedByCooldown_PublishesNothingAndAuditsSuppression()
    {
        var message = MonitoringTestDataFactory.CreateBatch();
        _patientCareplanStore.TryGet(message.PatientId).Returns(Careplan.ValveDisease);
        _policyResolver.Resolve(Careplan.ValveDisease).Returns(_policy);

        _policy.Evaluate(Arg.Any<BatchEvaluationContext>())
            .Returns(
                MonitoringTestDataFactory.CreatePolicyResult(
                    message,
                    Careplan.ValveDisease,
                    Severity.Critical,
                    true,
                    true
                )
            );

        var remaining = TimeSpan.FromMinutes(3);
        _cooldownService.Evaluate(message.PatientId, Severity.Critical, Arg.Any<DateTime>())
            .Returns(new CooldownCheckResult(IsSuppressed: true, RemainingCooldown: remaining));

        await _handler.Handle(message, _messageBus);

        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<PatientAlertRequested>());
        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<HospitalAlertRequested>());

        await _decisionAuditStore
            .Received(1)
            .RecordAuditEntry(
                Arg.Is<DecisionAuditEntry>(e =>
                    e.Severity == Severity.Critical
                    && e.SuppressedByCooldown
                    && e.RemainingCooldown == remaining
                    && !e.PatientAlertPublished
                    && !e.HospitalAlertPublished
                )
            );
    }

    [Fact]
    public async Task Handle_WhenNoSeverity_UsesNoneCareplanAndSkipsCooldownAndPublishing()
    {
        var message = MonitoringTestDataFactory.CreateBatch();
        _patientCareplanStore.TryGet(message.PatientId).Returns((Careplan?)null);
        _policyResolver.Resolve(Careplan.None).Returns(_policy);

        _policy.Evaluate(Arg.Any<BatchEvaluationContext>())
            .Returns(MonitoringTestDataFactory.CreatePolicyResult(message, Careplan.None, null, false, false));

        await _handler.Handle(message, _messageBus);

        _cooldownService
            .DidNotReceive()
            .Evaluate(Arg.Any<Guid>(), Arg.Any<Severity>(), Arg.Any<DateTime>());

        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<PatientAlertRequested>());
        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<HospitalAlertRequested>());

        await _decisionAuditStore
            .Received(1)
            .RecordAuditEntry(
                Arg.Is<DecisionAuditEntry>(e =>
                    e.Careplan == Careplan.None
                    && e.Severity == null
                    && !e.PatientAlertPublished
                    && !e.HospitalAlertPublished
                )
            );
    }
}