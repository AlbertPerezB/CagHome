using CagHome.Contracts;
using CagHome.Contracts.Enums;
using JasperFx.Core;
using NSubstitute;
using Wolverine.Tracking;
using Xunit;

namespace CagHome.NotificationService.Tests.Integration;

public class ClinicianResponseTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public ClinicianResponseTests(NotificationServiceFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    private static ClinicianResponseReceived CreateMessage() =>
        new ClinicianResponseReceived(
            AlertId: Guid.NewGuid(),
            CreatedAtUtc: DateTime.UtcNow,
            HospitalId: Guid.NewGuid(),
            Message: "Lay down and keep feet high. An ambulance is on the way.",
            ResponseId: Guid.NewGuid(),
            PatientId: Guid.NewGuid()
        );

    [Fact]
    public async Task Message_IsRoutedToHandler_AndExecutesSuccessfully()
    {
        var message = CreateMessage();

        var session = await _fixture
            .Host.TrackActivity()
            .Timeout(5.Seconds())
            .PublishMessageAndWaitAsync(message);

        Assert.NotNull(session.Executed.SingleMessage<ClinicianResponseReceived>());
    }
}
