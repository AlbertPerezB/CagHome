using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.PatientRegistryService.Application;
using CagHome.PatientRegistryService.Domain;
using CagHome.PatientRegistryService.Infrastructure;
using CagHome.PatientRegistryService.Tests.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Wolverine;

namespace CagHome.PatientRegistryService.Tests.UnitTests;

public class PatientStatusUpdateHandlerTests
{
    private readonly IPatientRegistryStore _store;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<PatientStatusUpdateRequested> _logger;
    private readonly PatientStatusUpdateHandler _handler;

    public PatientStatusUpdateHandlerTests()
    {
        _store = Substitute.For<IPatientRegistryStore>();
        _messageBus = Substitute.For<IMessageBus>();
        _logger = Substitute.For<ILogger<PatientStatusUpdateRequested>>();
        _handler = new PatientStatusUpdateHandler();
    }

    public static PatientStatusUpdateRequested CreateUpdateRequest(
        Guid? patientId = null,
        PatientStatus status = PatientStatus.Active,
        DateTime? updatedAtUtc = null
    ) =>
        new(
            PatientId: patientId ?? Guid.NewGuid(),
            PatientStatus: status,
            UpdatedAtUtc: updatedAtUtc ?? DateTime.UtcNow
        );

    [Fact]
    public async Task Handle_ShouldPassCorrectEntryToStore()
    {
        var patientId = Guid.NewGuid();
        var updatedAt = new DateTime(2026, 5, 12, 10, 0, 0, DateTimeKind.Utc);
        var message = CreateUpdateRequest(
            patientId: patientId,
            status: PatientStatus.Inactive,
            updatedAtUtc: updatedAt
        );

        _store
            .UpdatePatientData(Arg.Any<PatientRegistryEntry>())
            .Returns(FakeUpdateResult.Modified());

        await _handler.Handle(message, _store, _messageBus, _logger);

        await _store
            .Received(1)
            .UpdatePatientData(
                Arg.Is<PatientRegistryEntry>(e =>
                    e.PatientId == patientId
                    && e.Status == PatientStatus.Inactive
                    && e.LastUpdatedUtc == updatedAt
                )
            );
    }

    [Fact]
    public async Task Handle_WhenModified_ShouldPublish()
    {
        var message = CreateUpdateRequest();
        _store
            .UpdatePatientData(Arg.Any<PatientRegistryEntry>())
            .Returns(FakeUpdateResult.Modified());

        await _handler.Handle(message, _store, _messageBus, _logger);

        await _messageBus.Received(1).PublishAsync(Arg.Any<PatientStatusUpdated>());
    }

    [Fact]
    public async Task Handle_WhenUpserted_ShouldPublish()
    {
        var message = CreateUpdateRequest();
        _store
            .UpdatePatientData(Arg.Any<PatientRegistryEntry>())
            .Returns(FakeUpdateResult.Upserted());

        await _handler.Handle(message, _store, _messageBus, _logger);

        await _messageBus.Received(1).PublishAsync(Arg.Any<PatientStatusUpdated>());
    }

    [Fact]
    public async Task Handle_WhenNoChange_ShouldNotPublish()
    {
        var message = CreateUpdateRequest();
        _store
            .UpdatePatientData(Arg.Any<PatientRegistryEntry>())
            .Returns(FakeUpdateResult.NoChange());

        await _handler.Handle(message, _store, _messageBus, _logger);

        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<PatientStatusUpdated>());
    }

    [Fact]
    public async Task Handle_WhenUnacknowledged_ShouldNotPublish()
    {
        var message = CreateUpdateRequest();
        _store
            .UpdatePatientData(Arg.Any<PatientRegistryEntry>())
            .Returns(FakeUpdateResult.Unacknowledged());

        await _handler.Handle(message, _store, _messageBus, _logger);

        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<PatientStatusUpdated>());
    }

    [Fact]
    public async Task Handle_PublishedMessage_ShouldMirrorInput()
    {
        var patientId = Guid.NewGuid();
        var updatedAt = new DateTime(2026, 1, 15, 8, 30, 0, DateTimeKind.Utc);
        var message = CreateUpdateRequest(
            patientId: patientId,
            status: PatientStatus.Active,
            updatedAtUtc: updatedAt
        );

        _store
            .UpdatePatientData(Arg.Any<PatientRegistryEntry>())
            .Returns(FakeUpdateResult.Modified());

        PatientStatusUpdated? captured = null;
        await _messageBus.PublishAsync(Arg.Do<PatientStatusUpdated>(m => captured = m));

        await _handler.Handle(message, _store, _messageBus, _logger);

        Assert.NotNull(captured);
        Assert.Equal(patientId, captured!.PatientId);
        Assert.Equal(PatientStatus.Active, captured.PatientStatus);
        Assert.Equal(updatedAt, captured.UpdatedAtUtc);
    }

    [Fact]
    public async Task Handle_WhenStoreThrows_ShouldPropagateAndNotPublish()
    {
        var message = CreateUpdateRequest();
        _store
            .UpdatePatientData(Arg.Any<PatientRegistryEntry>())
            .ThrowsAsync(new MongoException("connection lost"));

        await Assert.ThrowsAsync<MongoException>(() =>
            _handler.Handle(message, _store, _messageBus, _logger)
        );

        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<PatientStatusUpdated>());
    }
}
