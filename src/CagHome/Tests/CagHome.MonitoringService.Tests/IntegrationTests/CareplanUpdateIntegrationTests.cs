using CagHome.Contracts.Enums;
using Wolverine.Tracking;

namespace CagHome.MonitoringService.Tests.Integration;

public class CareplanUpdateIntegrationTests : IClassFixture<MonitoringServiceFixture>
{
    private readonly MonitoringServiceFixture _fixture;

    public CareplanUpdateIntegrationTests(MonitoringServiceFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    [Fact]
    public async Task Message_IsRoutedToHandler_CareplanStateIsPersisted()
    {
        var message = new CareplanUpdateRequested(
            Careplan: Careplan.CoronaryArteryDisease,
            PatientId: Guid.NewGuid(),
            UpdatedAtUtc: DateTime.UtcNow
        );

        var session = await _fixture.Host.TrackActivity().PublishMessageAndWaitAsync(message);

        Assert.NotNull(session.Executed.SingleMessage<CareplanUpdateRequested>());

        var savedCareplan = await _fixture.PatientCareplanStore.TryGet(message.PatientId);
        Assert.Equal(Careplan.CoronaryArteryDisease, savedCareplan);
    }
}