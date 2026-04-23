using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.NotificationService.Domain;
using CagHome.NotificationService.Infrastructure;
using JasperFx.Core;
using NSubstitute;
using Wolverine.Tracking;
using Xunit;

namespace CagHome.NotificationService.Tests.Integration;

public class PatientAlertIntegrationTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public PatientAlertIntegrationTests(NotificationServiceFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    private static PatientAlertRequested CreateMessage() =>
        new PatientAlertRequested(
            AlertId: Guid.NewGuid(),
            DecidedAt: DateTime.UtcNow,
            Message: "High heart rate. Patient risks going into SVT",
            PatientId: Guid.NewGuid(),
            Severity: Severity.Critical
        );

    [Fact]
    public async Task Message_IsRoutedToHandler_AndExecutesSuccessfully()
    {
        var message = CreateMessage();

        var session = await _fixture
            .Host.TrackActivity()
            .Timeout(5.Seconds())
            .InvokeMessageAndWaitAsync(message);

        Assert.NotNull(session.Executed.SingleMessage<PatientAlertRequested>());
    }
}
