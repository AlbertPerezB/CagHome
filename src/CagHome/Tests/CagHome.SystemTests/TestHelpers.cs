using System.Net.Http.Json;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace CagHome.SystemTests;

public class TestHelpers
{
    private readonly AspireAppFixture _fixture;
    private readonly ITestOutputHelper _output;

    public TestHelpers(AspireAppFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    public async Task<string> InjectBatch(Guid patientId, object payload)
    {
        var response = await _fixture.Simulator.PostAsJsonAsync("/simulator/inject", payload);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var correlationId = body.GetProperty("correlationId").GetString()!;

        _output.WriteLine(
            $"Injected batch for patient {patientId}, correlationId: {correlationId}"
        );
        return correlationId;
    }

    public async Task<BsonDocument?> WaitForMonitoringAudit(
        string correlationId,
        int maxWaitSeconds = 15
    )
    {
        var collection = _fixture.MonitoringAuditDb.GetCollection<BsonDocument>(
            "DecisionAuditEntries"
        );
        var deadline = DateTime.UtcNow.AddSeconds(maxWaitSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("TraceId", correlationId);
            var entry = await collection.Find(filter).FirstOrDefaultAsync();

            if (entry != null)
                return entry;
            await Task.Delay(500);
        }

        return null;
    }

    public async Task<List<BsonDocument>> WaitForNotificationAudit(
        string correlationId,
        int expectedCount = 1,
        int maxWaitSeconds = 15
    )
    {
        var collection = _fixture.NotificationAuditDb.GetCollection<BsonDocument>("AuditEntries");
        var deadline = DateTime.UtcNow.AddSeconds(maxWaitSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("TraceId", correlationId);
            var entries = await collection.Find(filter).ToListAsync();

            if (entries.Count >= expectedCount)
                return entries;
            await Task.Delay(500);
        }

        return new List<BsonDocument>();
    }

    public static object NormalBatch(Guid patientId) =>
        new
        {
            schemaVersion = 1,
            appVersion = "1.0.0",
            patientId = patientId.ToString(),
            measurements = new object[]
            {
                new
                {
                    measurementId = Guid.NewGuid().ToString(),
                    type = "HeartRate",
                    value = 72.0,
                    unit = "Bpm",
                    deviceReported = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                    source = new
                    {
                        deviceId = "test-001",
                        deviceManufacturer = "TestHarness",
                        deviceModel = "v1",
                    },
                },
                new
                {
                    measurementId = Guid.NewGuid().ToString(),
                    type = "Spo2",
                    value = 97.0,
                    unit = "Percent",
                    deviceReported = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                    source = new
                    {
                        deviceId = "test-001",
                        deviceManufacturer = "TestHarness",
                        deviceModel = "v1",
                    },
                },
                new
                {
                    measurementId = Guid.NewGuid().ToString(),
                    type = "BodyTemperature",
                    value = 36.7,
                    unit = "C",
                    deviceReported = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                    source = new
                    {
                        deviceId = "test-001",
                        deviceManufacturer = "TestHarness",
                        deviceModel = "v1",
                    },
                },
            },
        };

    public static object WarningHeartRateBatch(Guid patientId) =>
        new
        {
            schemaVersion = 1,
            appVersion = "1.0.0",
            patientId = patientId.ToString(),
            measurements = new object[]
            {
                new
                {
                    measurementId = Guid.NewGuid().ToString(),
                    type = "HeartRate",
                    value = 110.0,
                    unit = "Bpm",
                    deviceReported = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                    source = new
                    {
                        deviceId = "test-001",
                        deviceManufacturer = "TestHarness",
                        deviceModel = "v1",
                    },
                },
                new
                {
                    measurementId = Guid.NewGuid().ToString(),
                    type = "Spo2",
                    value = 97.0,
                    unit = "Percent",
                    deviceReported = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                    source = new
                    {
                        deviceId = "test-001",
                        deviceManufacturer = "TestHarness",
                        deviceModel = "v1",
                    },
                },
            },
        };

    public static object CriticalHeartRateBatch(Guid patientId) =>
        new
        {
            schemaVersion = 1,
            appVersion = "1.0.0",
            patientId = patientId.ToString(),
            measurements = new object[]
            {
                new
                {
                    measurementId = Guid.NewGuid().ToString(),
                    type = "HeartRate",
                    value = 130.0,
                    unit = "Bpm",
                    deviceReported = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                    source = new
                    {
                        deviceId = "test-001",
                        deviceManufacturer = "TestHarness",
                        deviceModel = "v1",
                    },
                },
                new
                {
                    measurementId = Guid.NewGuid().ToString(),
                    type = "Spo2",
                    value = 97.0,
                    unit = "Percent",
                    deviceReported = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                    source = new
                    {
                        deviceId = "test-001",
                        deviceManufacturer = "TestHarness",
                        deviceModel = "v1",
                    },
                },
            },
        };
}
