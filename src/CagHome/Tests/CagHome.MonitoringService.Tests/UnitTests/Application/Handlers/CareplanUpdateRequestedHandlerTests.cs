using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Handlers;
using CagHome.MonitoringService.Infrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CagHome.MonitoringService.Tests.Application.Handlers;

public class CareplanUpdateRequestedHandlerTests
{
    private readonly IPatientCareplanStore _patientCareplanStore;
    private readonly ILogger<CareplanUpdateRequestedHandler> _logger;
    private readonly CareplanUpdateRequestedHandler _handler;

    public CareplanUpdateRequestedHandlerTests()
    {
        _patientCareplanStore = Substitute.For<IPatientCareplanStore>();
        _logger = Substitute.For<ILogger<CareplanUpdateRequestedHandler>>();
        _handler = new CareplanUpdateRequestedHandler();
    }

    [Fact]
    public async Task Handle_UpsertsCareplanForPatient()
    {
        var message = new CareplanUpdateRequested(
            Careplan: Careplan.Cardiomyopathy,
            PatientId: Guid.NewGuid(),
            UpdatedAtUtc: DateTime.UtcNow
        );

        await _handler.Handle(message, _patientCareplanStore, _logger);

        await _patientCareplanStore
            .Received(1)
            .Upsert(message.PatientId, message.Careplan, message.UpdatedAtUtc);
    }
}