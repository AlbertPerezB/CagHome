using CagHome.NotificationService.Application.Handlers;
using CagHome.NotificationService.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Tracking;

namespace CagHome.NotificationService.Tests.Integration;

public class NotificationServiceFixture : IAsyncLifetime
{
    public IHost Host { get; private set; } = null!;
    public IAuditStore AuditStore { get; private set; } = null!;
    public FakeEhrHttpHandler EhrHttpHandler { get; private set; } = null!;
    public IMqttPublisher MqttPublisher { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        AuditStore = Substitute.For<IAuditStore>();
        EhrHttpHandler = new FakeEhrHttpHandler();
        MqttPublisher = Substitute.For<IMqttPublisher>();

        Host = await Microsoft
            .Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseWolverine(options =>
            {
                // Discover handlers from the Notification Service
                options.Discovery.IncludeAssembly(typeof(HospitalAlertHandler).Assembly);

                // Match the real service's error handling policies
                options
                    .Policies.OnException<HttpRequestException>()
                    .RetryWithCooldown(
                        TimeSpan.FromMilliseconds(100), // shortened for tests
                        TimeSpan.FromMilliseconds(200),
                        TimeSpan.FromMilliseconds(300)
                    )
                    .Then.MoveToErrorQueue();

                options.Policies.OnException<BadHttpRequestException>().MoveToErrorQueue();

                options
                    .Policies.OnAnyException()
                    .RetryWithCooldown(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
                    .Then.MoveToErrorQueue();
            })
            .ConfigureServices(services =>
            {
                // Swap in test doubles
                services.AddSingleton<IAuditStore>(AuditStore);
                services.AddSingleton(MqttPublisher);

                services
                    .AddHttpClient(
                        "mock-ehr",
                        client =>
                        {
                            client.BaseAddress = new Uri("https://mock-ehr");
                        }
                    )
                    .ConfigurePrimaryHttpMessageHandler(() => EhrHttpHandler);
            })
            .StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Host.StopAsync();
        Host.Dispose();
    }

    public void Reset()
    {
        EhrHttpHandler.RespondWith(System.Net.HttpStatusCode.OK);
        MqttPublisher.ClearReceivedCalls();
    }
}
