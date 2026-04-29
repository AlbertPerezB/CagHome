using System.Net;
using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.NotificationService.Domain;
using JasperFx.Core;
using Wolverine.Tracking;
using Xunit;

namespace CagHome.NotificationService.Tests.Integration;

public class HospitalAlertIntegrationTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public HospitalAlertIntegrationTests(NotificationServiceFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
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

    [Fact]
    public async Task Message_IsRoutedToHandler_AndExecutesSuccessfully()
    {
        var message = CreateMessage();
        _fixture.Reset();

        var session = await _fixture
            .Host.TrackActivity()
            .Timeout(5.Seconds())
            .PublishMessageAndWaitAsync(message);

        // Wolverine routed it and the handler ran to completion
        Assert.NotNull(session.Executed.SingleMessage<HospitalAlertRequested>());
    }

    [Fact]
    public async Task OnSuccess_PostsToEhrAlertEndpoint()
    {
        var message = CreateMessage();
        _fixture.Reset();

        await _fixture
            .Host.TrackActivity()
            .Timeout(10.Seconds())
            .InvokeMessageAndWaitAsync(message);

        Assert.Equal(1, _fixture.EhrHttpHandler.CallCount);
        Assert.Equal("/alerts", _fixture.EhrHttpHandler.LastRequest?.RequestUri?.AbsolutePath);
    }

    [Fact] // HttpRequestException (500-errors) should retry multiple times (initial + 3 retries from policy)
    public async Task OnHttpFailure_RetriesBeforeMovingToErrorQueue()
    {
        var message = CreateMessage();
        _fixture.Reset();
        _fixture.EhrHttpHandler.RespondWith(HttpStatusCode.InternalServerError);

        var session = await _fixture
            .Host.TrackActivity()
            .DoNotAssertOnExceptionsDetected()
            .Timeout(30.Seconds())
            .PublishMessageAndWaitAsync(message);

        Assert.True(
            _fixture.EhrHttpHandler.CallCount > 1,
            $"Expected retries but only got {_fixture.EhrHttpHandler.CallCount} call(s)"
        );
    }

    [Fact] // BadHttpRequestException (400-errors) should skip retries per the policy
    public async Task OnBadRequest_MovesDirectlyToErrorQueueWithoutRetry()
    {
        var message = CreateMessage();
        _fixture.Reset();
        _fixture.EhrHttpHandler.RespondWith(HttpStatusCode.BadRequest);

        await _fixture
            .Host.TrackActivity()
            .DoNotAssertOnExceptionsDetected()
            .Timeout(10.Seconds())
            .PublishMessageAndWaitAsync(message);

        Assert.Equal(1, _fixture.EhrHttpHandler.CallCount);
    }
}
