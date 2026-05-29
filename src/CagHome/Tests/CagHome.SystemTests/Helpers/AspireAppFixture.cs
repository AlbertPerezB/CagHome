using System.Net.Http.Json;
using Aspire.Hosting;
using CagHome.Contracts;
using CagHome.SystemTests.TestClasses;
using ImTools;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StackExchange.Redis;
using Wolverine;
using Wolverine.RabbitMQ;

namespace CagHome.SystemTests.Helpers;

public class AspireAppFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(120);

    private DistributedApplication _app = null!;
    public HttpClient Simulator = null!;
    public HttpClient MockEhr = null!;
    public IMongoCollection<BsonDocument> MonitoringAudit = null!;
    public IMongoCollection<BsonDocument> NotificationAudit = null!;
    public IMongoCollection<BsonDocument> PatientRegistry = null!;
    public IMongoCollection<BsonDocument> MonitoringCareplans = null!;
    public IDatabase RedisCache = null!;
    private IConnectionMultiplexer _connectionMultiplexer = null!;
    private IHost? _publisherHost;

    public async Task InitializeAsync()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

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

        Simulator = _app.CreateHttpClient("mock-application");
        MockEhr = _app.CreateHttpClient("mock-ehr");

        await GetDatabases();
        await CleanTestData();

        await WaitForSystemReady(ct);

        foreach (var patient in TestPatient.All())
        {
            await SeedPatient(patient);
        }
    }

    /// <summary>
    /// Seeds the patient data in the patient registry db, careplans db and Redis cache.
    /// </summary>
    /// <returns>Task when finished.</returns>
    public async Task SeedPatient(TestPatient patient)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", patient.PatientId);

        var patientRegistryUpdate = Builders<BsonDocument>
            .Update.Set("Status", patient.Status.ToString())
            .Set("LastUpdatedUtc", DateTime.UtcNow);
        var result = await PatientRegistry.UpdateOneAsync(
            filter,
            patientRegistryUpdate,
            new UpdateOptions { IsUpsert = true }
        );

        if (!result.IsAcknowledged || (result.ModifiedCount == 0 && result.UpsertedId == null))
        {
            throw new Exception(
                $"Failed to upsert patient {patient.PatientId} into Patient Registry."
            );
        }

        var careplansUpdate = Builders<BsonDocument>
            .Update.Set("Careplan", patient.Careplan.ToString())
            .Set("UpdatedAtUtc", DateTime.UtcNow);

        await MonitoringCareplans.UpdateOneAsync(
            filter,
            careplansUpdate,
            new UpdateOptions { IsUpsert = true }
        );

        await RedisCache.StringSetAsync(
            $"patient:{patient.PatientId}:status",
            patient.Status.ToString()
        );
    }

    private async Task GetDatabases()
    {
        var mongoConnection = await _app.GetConnectionStringAsync("mongo");
        var mongoClient = new MongoClient(mongoConnection);
        MonitoringAudit = mongoClient
            .GetDatabase("monitoring-audit")
            .GetCollection<BsonDocument>("DecisionAuditEntries");
        NotificationAudit = mongoClient
            .GetDatabase("notification-audit")
            .GetCollection<BsonDocument>("NotificationAuditEntries");
        PatientRegistry = mongoClient
            .GetDatabase("patient-registry")
            .GetCollection<BsonDocument>("PatientData");
        MonitoringCareplans = mongoClient
            .GetDatabase("monitoring-patient-careplans")
            .GetCollection<BsonDocument>("PatientCareplans");

        var redisConnection = await _app.GetConnectionStringAsync("patient-cache");
        _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(redisConnection!);
        RedisCache = _connectionMultiplexer.GetDatabase();
    }

    public async Task<string> GetRabbitConnectionString()
    {
        return await _app.GetConnectionStringAsync("rabbitmq-broker")
            ?? throw new Exception("Could not get RabbitMQ connection string");
    }

    /// <summary>
    /// Creates a lightweight Wolverine host connected to the system's RabbitMQ instance,
    /// used to inject messages directly into queues bypassing the EHR poller.
    /// </summary>
    /// <returns>The message bus instance.</returns>
    public async Task<IMessageBus> GetTestMessageBus()
    {
        if (_publisherHost != null)
            return _publisherHost.Services.GetRequiredService<IMessageBus>();

        var connectionString = await GetRabbitConnectionString();
        _publisherHost = Host.CreateDefaultBuilder()
            .UseWolverine(opts =>
            {
                opts.UseRabbitMq(new Uri(connectionString));
                opts.PublishMessage<PatientStatusUpdateRequested>()
                    .ToRabbitQueue("patient-registry.patient-status-update");
            })
            .Build();

        await _publisherHost.StartAsync();
        return _publisherHost.Services.GetRequiredService<IMessageBus>();
    }

    private async Task CleanTestData()
    {
        foreach (var patient in TestPatient.All())
        {
            var filter = Builders<BsonDocument>.Filter.Eq("PatientId", patient.PatientId);
            await MonitoringAudit.DeleteManyAsync(filter);
            await NotificationAudit.DeleteManyAsync(filter);
        }
    }

    private async Task WaitForSystemReady(CancellationToken ct)
    {
        var deadline = DateTime.UtcNow.AddSeconds(60);
        var probePatientId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Seed a throwaway patient
        await SeedPatient(
            new TestPatient
            {
                PatientId = probePatientId,
                Status = CagHome.Contracts.Enums.PatientStatus.Active,
                Careplan = CagHome.Contracts.Enums.Careplan.None,
            }
        );

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var payload = TestHelpers.NormalBatch(probePatientId);
                await Simulator.PostAsJsonAsync("/simulator/inject", payload, ct);
                await Task.Delay(3000, ct);

                var filter = Builders<BsonDocument>.Filter.Eq("PatientId", probePatientId);
                var entry = await MonitoringAudit.Find(filter).FirstOrDefaultAsync(ct);
                if (entry != null)
                {
                    // Clean up probe data
                    await MonitoringAudit.DeleteManyAsync(filter, ct);
                    return;
                }
            }
            catch { }

            await Task.Delay(2000, ct);
        }

        throw new TimeoutException("System did not become ready within 60 seconds");
    }

    public async Task DisposeAsync()
    {
        _connectionMultiplexer?.Dispose();
        Simulator?.Dispose();
        MockEhr?.Dispose();
        _publisherHost?.Dispose();
        if (_app != null)
            await _app.DisposeAsync();
    }
}
