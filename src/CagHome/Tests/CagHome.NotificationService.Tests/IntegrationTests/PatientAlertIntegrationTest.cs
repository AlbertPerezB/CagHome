using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.NotificationService.Tests.Helpers;
using JasperFx.Core;
using Wolverine.Tracking;

namespace CagHome.NotificationService.Tests.IntegrationTests;

public class PatientAlertIntegrationTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public PatientAlertIntegrationTests(NotificationServiceFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    [Fact]
    public async Task Message_IsRoutedToHandler_AndExecutesSuccessfully()
    {
        var message = TestDataFactory.CreatePatientAlertRequested();

        var session = await _fixture
            .Host.TrackActivity()
            .Timeout(5.Seconds())
            .PublishMessageAndWaitAsync(message);

        Assert.NotNull(session.Executed.SingleMessage<PatientAlertRequested>());
    }
}
