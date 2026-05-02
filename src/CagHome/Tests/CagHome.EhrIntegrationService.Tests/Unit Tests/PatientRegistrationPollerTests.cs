using CagHome.Contracts;
using CagHome.EhrIntegrationService.Application.Pollers;
using CagHome.EhrIntegrationService.Domain;
using CagHome.EhrIntegrationService.Infrastructure;
using CagHome.EhrIntegrationService.Tests.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CagHome.EhrIntegrationService.Tests.UnitTests;

public class PatientRegistrationPollerTests
{
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<PatientRegistrationPoller> _logger;
    private readonly FakeEhrHttpHandler _httpHandler;
    private readonly IHttpClientFactory _httpClientFactory;

    public PatientRegistrationPollerTests()
    {
        _publisher = Substitute.For<IRabbitMqPublisher>();
        _logger = Substitute.For<ILogger<PatientRegistrationPoller>>();
        _httpHandler = new FakeEhrHttpHandler();
        var client = new HttpClient(_httpHandler) { BaseAddress = new Uri("https://mock-ehr") };
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory.CreateClient("mock-ehr").Returns(client);
    }

    private TestablePatientRegistrationPoller CreatePoller() =>
        new(_httpClientFactory, _logger, _publisher);

    [Fact]
    public async Task PublishesBothMessagesPerPatient()
    {
        var patient = new PatientRegistrationDto(
            Careplan: Contracts.Enums.Careplan.Cardiomyopathy,
            PatientId: Guid.NewGuid(),
            Status: Contracts.Enums.PatientStatus.Active,
            UpdatedAtUtc: DateTime.UtcNow
        );
        _httpHandler.RespondWithJson(new List<PatientRegistrationDto> { patient });

        using var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            cts.Cancel();
        });

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            CreatePoller().ExecuteAsync(cts.Token)
        );

        await _publisher
            .Received(1)
            .PublishCareplanUpdateRequested(Arg.Any<CareplanUpdateRequested>());
        await _publisher
            .Received(1)
            .PublishPatientStatusUpdateRequested(Arg.Any<PatientStatusUpdateRequested>());
    }

    [Fact]
    public async Task MapsFieldsCorrectlyOntoContracts()
    {
        var patient = new PatientRegistrationDto(
            Careplan: Contracts.Enums.Careplan.Cardiomyopathy,
            PatientId: Guid.NewGuid(),
            Status: Contracts.Enums.PatientStatus.Active,
            UpdatedAtUtc: DateTime.UtcNow
        );
        _httpHandler.RespondWithJson(new List<PatientRegistrationDto> { patient });

        using var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            cts.Cancel();
        });

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            CreatePoller().ExecuteAsync(cts.Token)
        );

        await _publisher
            .Received(1)
            .PublishCareplanUpdateRequested(
                Arg.Is<CareplanUpdateRequested>(m =>
                    m.PatientId == patient.PatientId && m.Careplan == patient.Careplan
                )
            );

        await _publisher
            .Received(1)
            .PublishPatientStatusUpdateRequested(
                Arg.Is<PatientStatusUpdateRequested>(m =>
                    m.PatientId == patient.PatientId && m.PatientStatus == patient.Status
                )
            );
    }

    [Fact]
    public async Task MultiplePatients_PublishesForEach()
    {
        _httpHandler.RespondWithJson(
            new List<PatientRegistrationDto>
            {
                new PatientRegistrationDto(
                    Careplan: Contracts.Enums.Careplan.ValveDisease,
                    PatientId: Guid.NewGuid(),
                    Status: Contracts.Enums.PatientStatus.Deceased,
                    UpdatedAtUtc: DateTime.UtcNow
                ),
                new PatientRegistrationDto(
                    Careplan: Contracts.Enums.Careplan.Cardiomyopathy,
                    PatientId: Guid.NewGuid(),
                    Status: Contracts.Enums.PatientStatus.Active,
                    UpdatedAtUtc: DateTime.UtcNow
                ),
            }
        );

        using var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            cts.Cancel();
        });

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            CreatePoller().ExecuteAsync(cts.Token)
        );

        await _publisher
            .Received(2)
            .PublishCareplanUpdateRequested(Arg.Any<CareplanUpdateRequested>());
        await _publisher
            .Received(2)
            .PublishPatientStatusUpdateRequested(Arg.Any<PatientStatusUpdateRequested>());
    }

    [Fact]
    public async Task EmptyResponse_PublishesNothing()
    {
        _httpHandler.RespondWithJson(new List<PatientRegistrationDto>());

        using var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            cts.Cancel();
        });

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            CreatePoller().ExecuteAsync(cts.Token)
        );

        await _publisher
            .DidNotReceive()
            .PublishCareplanUpdateRequested(Arg.Any<CareplanUpdateRequested>());
        await _publisher
            .DidNotReceive()
            .PublishPatientStatusUpdateRequested(Arg.Any<PatientStatusUpdateRequested>());
    }

    [Fact]
    public async Task HttpFailure_ContinuesWithoutCrashing()
    {
        _httpHandler.ThrowOnNextRequest(new HttpRequestException("Connection refused"));

        using var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            cts.Cancel();
        });

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            CreatePoller().ExecuteAsync(cts.Token)
        );

        await _publisher
            .DidNotReceive()
            .PublishCareplanUpdateRequested(Arg.Any<CareplanUpdateRequested>());
        await _publisher
            .DidNotReceive()
            .PublishPatientStatusUpdateRequested(Arg.Any<PatientStatusUpdateRequested>());
    }
}
