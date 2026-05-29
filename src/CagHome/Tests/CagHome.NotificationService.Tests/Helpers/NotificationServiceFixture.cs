using CagHome.NotificationService.Application.Handlers;
using CagHome.NotificationService.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Wolverine;
using Wolverine.ErrorHandling;

namespace CagHome.NotificationService.Tests.Helpers;

/// <summary>
/// A service fixture for testing the Notification Service. It sets up a test host with the same configuration as the real service,
/// but replaces external dependencies with test doubles. It also provides methods for resetting the state of the test doubles between tests.
/// </summary>
public class NotificationServiceFixture : IAsyncLifetime
{
    public IHost Host { get; private set; } = null!;
    public IAuditStore AuditStore { get; private set; } = null!;
    public FakeEhrHttpHandler EhrHttpHandler { get; private set; } = null!;
    public IMqttPublisher MqttPublisher { get; private set; } = null!;

    /// <summary>
    /// Asynchronously initializes the test host environment and configures service dependencies for integration
    /// testing.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
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

    /// <summary>
    /// Disposes of the test host and its resources. Called automatically by xUnit after all tests have completed.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async Task DisposeAsync()
    {
        await Host.StopAsync();
        Host.Dispose();
    }

    /// <summary>
    /// Resets the internal state of the HTTP handler and clears all received calls from the MQTT publisher.
    /// </summary>
    public void Reset()
    {
        EhrHttpHandler.Reset();
        MqttPublisher.ClearReceivedCalls();
    }
}
