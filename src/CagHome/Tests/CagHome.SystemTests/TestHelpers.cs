using System.Net.Http.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace CagHome.SystemTests;

public class TestHelpers{
    private readonly AspireAppFixture _fixture;
    private readonly ITestOutputHelper _output;

    public TestHelpers(AspireAppFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    /// <summary>
    /// Returns the total count of monitoring audit entries.
    /// </summary>
    public async Task<long> GetMonitoringAuditCount()
    {
        return await _fixture.MonitoringAudit.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
    }

    /// <summary>
    /// Injects a batch via the simulator endpoint.
    /// Returns the correlationId to use for filtering audit entries.
    /// </summary>
    public async Task<Guid> InjectBatch(Guid patientId, object payload)
    {
        var response = await _fixture.Simulator.PostAsJsonAsync("/simulator/inject", payload);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<InjectResponse>();
        _output.WriteLine($"Injected batch for patient {patientId} with correlation id {responseBody!.CorrelationId}");
        return responseBody.CorrelationId;
    }

    public async Task<BsonDocument?> WaitForMonitoringAudit(
        Guid correlationId,
        int maxWaitSeconds = 30
    )
    {
        var deadline = DateTime.UtcNow.AddSeconds(maxWaitSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("CorrelationId", correlationId);
            var entry = await _fixture
                .MonitoringAudit.Find(filter)
                .SortByDescending(d => d["TimestampUtc"])
                .FirstOrDefaultAsync();

            if (entry != null)
                return entry;
            await Task.Delay(500);
        }

        return null;
    }

    public async Task<List<BsonDocument>> WaitForNotificationAudit(
        Guid correlationId,
        int expectedCount = 1,
        int maxWaitSeconds = 15
    )
    {
        var deadline = DateTime.UtcNow.AddSeconds(maxWaitSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("CorrelationId", correlationId);
            var entries = await _fixture.NotificationAudit.Find(filter).ToListAsync();

            if (entries.Count >= expectedCount)
                return entries;
            await Task.Delay(500);
        }

        return new List<BsonDocument>();
    }

    // ── Payload builders ─────────────────────────────

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

public record InjectResponse(Guid CorrelationId);
