using CagHome.Contracts;
using CagHome.EhrIntegrationService.Application.Pollers;
using CagHome.EhrIntegrationService.Domain;
using CagHome.EhrIntegrationService.Infrastructure;
using CagHome.EhrIntegrationService.Tests.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CagHome.EhrIntegrationService.Tests.UnitTests;

public class ClinicianResponsePollerTests
{
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<ClinicianResponsePoller> _logger;
    private readonly FakeEhrHttpHandler _httpHandler;
    private readonly IHttpClientFactory _httpClientFactory;

    public ClinicianResponsePollerTests()
    {
        _publisher = Substitute.For<IRabbitMqPublisher>();
        _logger = Substitute.For<ILogger<TestableClinicianResponsePoller>>();
        _httpHandler = new FakeEhrHttpHandler();
        var client = new HttpClient(_httpHandler) { BaseAddress = new Uri("https://mock-ehr") };
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory.CreateClient("mock-ehr").Returns(client);
    }

    private TestableClinicianResponsePoller CreatePoller() =>
        new(_httpClientFactory, _logger, _publisher);

    [Fact]
    public async Task PublishesOneMessagePerResponse()
    {
        _httpHandler.RespondWithJson(
            new List<ClinicianResponseDto>
            {
                new ClinicianResponseDto(
                    AlertId: Guid.NewGuid(),
                    CreatedAtUtc: DateTime.UtcNow,
                    Message: "Increase dosage",
                    PatientId: Guid.NewGuid(),
                    ResponseId: Guid.NewGuid()
                ),
                new ClinicianResponseDto(
                    AlertId: Guid.NewGuid(),
                    CreatedAtUtc: DateTime.UtcNow,
                    Message: "Monitor closely",
                    PatientId: Guid.NewGuid(),
                    ResponseId: Guid.NewGuid()
                ),
            }
        );

        using var cts = new CancellationTokenSource();
        var poller = CreatePoller();

        // Cancel immediately after one iteration completes
        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            cts.Cancel();
        });

        await Assert.ThrowsAsync<TaskCanceledException>(() => poller.ExecuteAsync(cts.Token));

        await _publisher
            .Received(2)
            .PublishClinicianResponseReceived(Arg.Any<ClinicianResponseReceived>());
    }

    [Fact]
    public async Task MapsFieldsCorrectlyOntoContract()
    {
        var dto = new ClinicianResponseDto(
            AlertId: Guid.NewGuid(),
            CreatedAtUtc: DateTime.UtcNow,
            Message: "Reduce activity",
            PatientId: Guid.NewGuid(),
            ResponseId: Guid.NewGuid()
        );
        _httpHandler.RespondWithJson(new List<ClinicianResponseDto> { dto });

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(500));
        var poller = CreatePoller();

        try
        {
            await poller.ExecuteAsync(cts.Token);
        }
        catch (OperationCanceledException) { }

        await _publisher
            .Received(1)
            .PublishClinicianResponseReceived(
                Arg.Is<ClinicianResponseReceived>(m =>
                    m.ResponseId == dto.ResponseId
                    && m.AlertId == dto.AlertId
                    && m.PatientId == dto.PatientId
                    && m.Message == dto.Message
                )
            );
    }

    [Fact]
    public async Task EmptyResponse_PublishesNothing()
    {
        _httpHandler.RespondWithJson(new List<ClinicianResponseDto>());

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(500));
        var poller = CreatePoller();

        try
        {
            await poller.ExecuteAsync(cts.Token);
        }
        catch (OperationCanceledException) { }

        await _publisher
            .DidNotReceive()
            .PublishClinicianResponseReceived(Arg.Any<ClinicianResponseReceived>());
    }

    [Fact]
    public async Task HttpFailure_ContinuesWithoutCrashing()
    {
        _httpHandler.ThrowOnNextRequest(new HttpRequestException("Connection refused"));

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(500));
        var poller = CreatePoller();

        try
        {
            await poller.StartAsync(cts.Token);
        }
        catch (OperationCanceledException) { }

        // Poller survived — no unhandled exception
        await _publisher
            .DidNotReceive()
            .PublishClinicianResponseReceived(Arg.Any<ClinicianResponseReceived>());
    }
}
