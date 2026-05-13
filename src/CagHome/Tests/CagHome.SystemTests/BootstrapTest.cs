using Microsoft.Extensions.Logging;

namespace CagHome.SystemTests;

public class SystemBootstrapTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    [Fact]
    public async Task AllServicesStartSuccessfully()
    {
        // Arrange — spin up the entire system
        using var cts = new CancellationTokenSource(DefaultTimeout);
        var ct = cts.Token;

        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.CagHome_AppHost>(ct);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
            logging.AddFilter("CagHome", LogLevel.Debug);
        });

        await using var app = await appHost.BuildAsync(ct);
        await app.StartAsync(ct);

        // Act & Assert — wait for key services to be running
        await app.ResourceNotifications.WaitForResourceAsync(
            "ingestion-service",
            KnownResourceStates.Running,
            ct
        );

        await app.ResourceNotifications.WaitForResourceAsync(
            "monitoring",
            KnownResourceStates.Running,
            ct
        );

        await app.ResourceNotifications.WaitForResourceAsync(
            "notification",
            KnownResourceStates.Running,
            ct
        );

        await app.ResourceNotifications.WaitForResourceAsync(
            "patient-registry-service",
            KnownResourceStates.Running,
            ct
        );

        await app.ResourceNotifications.WaitForResourceAsync(
            "simulator",
            KnownResourceStates.Running,
            ct
        );
    }
}
