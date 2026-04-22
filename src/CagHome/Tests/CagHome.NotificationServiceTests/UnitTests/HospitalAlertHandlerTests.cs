using System.Net;
using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.NotificationService.Application.Handlers;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Wolverine.Transports;

namespace CagHome.NotificationService.Tests.Application.Handlers;

public class HospitalAlertHandlerTests
{
    private readonly IAuditStore _auditStore;
    private readonly ILogger<HospitalAlertHandler> _logger;
    private readonly HospitalAlertHandler _handler;

    public HospitalAlertHandlerTests()
    {
        _auditStore = Substitute.For<IAuditStore>();
        _logger = Substitute.For<ILogger<HospitalAlertHandler>>();
        _handler = new HospitalAlertHandler();
    }

    private static HospitalAlertRequested CreateMessage() =>
        new HospitalAlertRequested(
            AlertId: Guid.NewGuid(),
            DecidedAt: DateTime.UtcNow,
            HospitalId: Guid.NewGuid(),
            Message: "High heart rate. Patient risks going into SVT",
            PatientId: Guid.NewGuid(),
            Severity: Severity.Critical
        );

    private static IHttpClientFactory CreateHttpClientFactory(HttpStatusCode statusCode)
    {
        var handler = new FakeHttpMessageHandler(statusCode);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://mock-ehr") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("mock-ehr").Returns(client);
        return factory;
    }

    [Fact]
    public async Task Handle_OnSuccess_RecordsAttemptedThenDelivered()
    {
        var message = CreateMessage();
        var factory = CreateHttpClientFactory(HttpStatusCode.OK);

        await _handler.Handle(message, factory, _logger, _auditStore);

        await _auditStore.Received(2).RecordAuditEntry(Arg.Any<AuditEntry>());
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
    public async Task Handle_OnHttpFailure_RecordsAttemptedThenFailedAndThrows()
    {
        var message = CreateMessage();
        var factory = CreateHttpClientFactory(HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _handler.Handle(message, factory, _logger, _auditStore)
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

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;

    public FakeHttpMessageHandler(HttpStatusCode statusCode) => _statusCode = statusCode;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(new HttpResponseMessage(_statusCode));
    }
}
