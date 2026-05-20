using CagHome.Contracts;
using CagHome.NotificationService.Application.Handlers;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;
using CagHome.NotificationService.Tests.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CagHome.NotificationService.Tests.UnitTests;

public class ClinicianResponseHandlerTests
{
    private readonly IAuditStore _auditStore;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly ILogger<ClinicianResponseReceived> _logger;
    private readonly ClinicianResponseHandler _handler;

    public ClinicianResponseHandlerTests()
    {
        _auditStore = Substitute.For<IAuditStore>();
        _mqttPublisher = Substitute.For<IMqttPublisher>();
        _logger = Substitute.For<ILogger<ClinicianResponseReceived>>();
        _handler = new ClinicianResponseHandler();
    }

    [Fact]
    public async Task Handle_OnSuccess_RecordsAttemptedThenDelivered()
    {
        var message = TestDataFactory.CreateClinicianResponseReceived();

        await _handler.Handle(message, _mqttPublisher, _auditStore, _logger);

        Received.InOrder(async () =>
        {
            await _auditStore.RecordAuditEntry(
                Arg.Is<AuditEntry>(e => e.DeliveryStatus == DeliveryStatus.Attempted)
            );
            await _auditStore.RecordAuditEntry(
                Arg.Is<AuditEntry>(e => e.DeliveryStatus == DeliveryStatus.Delivered)
            );
        });
    }

    [Fact]
    public async Task Handle_OnSuccess_PublishesToCorrectPatient()
    {
        var message = TestDataFactory.CreateClinicianResponseReceived();

        await _handler.Handle(message, _mqttPublisher, _auditStore, _logger);

        await _mqttPublisher.Received(1).Publish(message.PatientId, Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_OnMqttFailure_RecordsFailedAndThrows()
    {
        var message = TestDataFactory.CreateClinicianResponseReceived();
        _mqttPublisher
            .When(x => x.Publish(Arg.Any<Guid>(), Arg.Any<object>()))
            .Throw(new Exception("MQTT connection lost"));

        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(message, _mqttPublisher, _auditStore, _logger)
        );

        Received.InOrder(async () =>
        {
            await _auditStore.RecordAuditEntry(
                Arg.Is<AuditEntry>(e => e.DeliveryStatus == DeliveryStatus.Attempted)
            );
            await _auditStore.RecordAuditEntry(
                Arg.Is<AuditEntry>(e => e.DeliveryStatus == DeliveryStatus.Failed)
            );
        });
    }
}
