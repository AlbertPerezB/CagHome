using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using StackExchange.Redis;

namespace CagHome.SystemTests;

public sealed class AspireAppFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(120);

    private DistributedApplication _app = null!;
    public HttpClient Simulator = null!;
    public IMongoDatabase MonitoringAuditDb = null!;
    public IMongoDatabase NotificationAuditDb = null!;
    public IMongoDatabase PatientRegistryDb = null!;
    public ConnectionMultiplexer Redis = null!;

    public async Task InitializeAsync()
    {
        var cts = new CancellationTokenSource(StartupTimeout);
        var ct = cts.Token;

        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.CagHome_AppHost>(ct);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
            logging.AddFilter("CagHome", LogLevel.Debug);
        });

        _app = await appHost.BuildAsync(ct);
        await _app.StartAsync(ct);

        await _app.ResourceNotifications.WaitForResourceAsync(
            "ingestionservice",
            KnownResourceStates.Running,
            ct
        );
        await _app.ResourceNotifications.WaitForResourceAsync(
            "monitoring",
            KnownResourceStates.Running,
            ct
        );
        await _app.ResourceNotifications.WaitForResourceAsync(
            "simulator",
            KnownResourceStates.Running,
            ct
        );

        Simulator = _app.CreateHttpClient("simulator");

        var mongoConnection = await _app.GetConnectionStringAsync("mongo");
        var mongoClient = new MongoClient(mongoConnection);
        MonitoringAuditDb = mongoClient.GetDatabase("monitoring-audit");
        NotificationAuditDb = mongoClient.GetDatabase("NotificationService");
        PatientRegistryDb = mongoClient.GetDatabase("PatientRegistry");

        var redisConnection = await _app.GetConnectionStringAsync("patient-cache");
        Redis = await ConnectionMultiplexer.ConnectAsync(redisConnection!);
    }

    public async Task DisposeAsync()
    {
        Redis?.Dispose();
        Simulator?.Dispose();
        if (_app != null)
            await _app.DisposeAsync();
    }
}
