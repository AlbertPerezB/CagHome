using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Tests.Helpers;
using Wolverine.Tracking;

namespace CagHome.MonitoringService.Tests.Integration;

public class BatchReceivedIntegrationTests : IClassFixture<MonitoringServiceFixture>
{
    private readonly MonitoringServiceFixture _fixture;

    public BatchReceivedIntegrationTests(MonitoringServiceFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    [Fact]
    public async Task CriticalBatch_ProducesPublishedAlertFlags_AndAuditEntry()
    {
        var patientId = Guid.NewGuid();
        await _fixture.PatientCareplanStore.Upsert(patientId, Careplan.Cardiomyopathy, DateTime.UtcNow);

        var message = MonitoringTestDataFactory.CreateBatch(patientId, heartRate: 130);

        var session = await _fixture.Host.TrackActivity().PublishMessageAndWaitAsync(message);

        Assert.NotNull(session.Executed.SingleMessage<BatchReceived>());

        var auditEntry = Assert.Single(_fixture.DecisionAuditStore.Entries);
        Assert.Equal(patientId, auditEntry.PatientId);
        Assert.Equal(message.BatchId, auditEntry.BatchId);
        Assert.Equal(message.CorrelationId, auditEntry.CorrelationId);
        Assert.Equal(Severity.Critical, auditEntry.Severity);
        Assert.True(auditEntry.PatientAlertPublished);
        Assert.True(auditEntry.HospitalAlertPublished);
        Assert.False(auditEntry.SuppressedByCooldown);
        Assert.NotEmpty(auditEntry.Reasons);
    }

    [Fact]
    public async Task ConsecutiveCriticalBatches_SecondIsSuppressedByCooldown()
    {
        var patientId = Guid.NewGuid();
        await _fixture.PatientCareplanStore.Upsert(patientId, Careplan.Cardiomyopathy, DateTime.UtcNow);

        var firstMessage = MonitoringTestDataFactory.CreateBatch(patientId, heartRate: 130);
        var secondMessage = MonitoringTestDataFactory.CreateBatch(patientId, heartRate: 132);

        await _fixture.Host.TrackActivity().PublishMessageAndWaitAsync(firstMessage);
        await _fixture.Host.TrackActivity().PublishMessageAndWaitAsync(secondMessage);

        var auditEntries = _fixture
            .DecisionAuditStore
            .Entries.Where(e => e.PatientId == patientId)
            .OrderBy(e => e.TimestampUtc)
            .ToList();

        Assert.Equal(2, auditEntries.Count);

        var firstAudit = auditEntries[0];
        var secondAudit = auditEntries[1];

        Assert.False(firstAudit.SuppressedByCooldown);
        Assert.True(firstAudit.PatientAlertPublished);
        Assert.True(firstAudit.HospitalAlertPublished);

        Assert.True(secondAudit.SuppressedByCooldown);
        Assert.False(secondAudit.PatientAlertPublished);
        Assert.False(secondAudit.HospitalAlertPublished);
        Assert.NotNull(secondAudit.RemainingCooldown);
    }
}