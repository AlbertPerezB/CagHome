using System.Net.Http.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace CagHome.SystemTests.Helpers;

public class TestHelpers
{
    private readonly AspireAppFixture _fixture;
    private readonly ITestOutputHelper _output;

    public TestHelpers(AspireAppFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    /// <summary>
    /// Injects a batch via the simulator endpoint.
    /// Returns the timestamp from just before injection (used to filter audit entries).
    /// </summary>
    public async Task<DateTime> InjectBatch(Guid patientId, object payload)
    {
        var beforeUtc = DateTime.UtcNow;

        var response = await _fixture.Simulator.PostAsJsonAsync("/simulator/inject", payload);
        response.EnsureSuccessStatusCode();

        _output.WriteLine($"Injected batch for patient {patientId}");
        return beforeUtc;
    }

    public async Task PostClinicianResponse(
        Guid alertId,
        Guid patientId,
        Guid hospitalId,
        string message
    )
    {
        var payload = new
        {
            AlertId = alertId,
            CreatedAtUtc = DateTime.UtcNow,
            HospitalId = hospitalId,
            Message = message,
            PatientId = patientId,
            ResponseId = Guid.NewGuid(),
        };

        var responseMessage = await _fixture.MockEhr.PostAsJsonAsync(
            "/mock/clinician-response",
            payload
        );
        responseMessage.EnsureSuccessStatusCode();
        _output.WriteLine($"Posted clinician response for alert {alertId}");
    }

    public async Task RegisterPatientInMockEHR(Guid patientId, int careplan, int status)
    {
        var payload = new
        {
            PatientId = patientId,
            UpdatedAtUtc = DateTime.UtcNow.ToString("O"),
            Careplan = careplan,
            Status = status,
        };
        var responseMessage = await _fixture.MockEhr.PostAsJsonAsync("/mock/patient", payload);
        responseMessage.EnsureSuccessStatusCode();
        _output.WriteLine($"Registered patient {patientId} in mock EHR");
    }

    public async Task<BsonDocument?> WaitForMonitoringAudit(
        Guid patientId,
        DateTime afterUtc,
        int maxWaitSeconds = 30
    )
    {
        var deadline = DateTime.UtcNow.AddSeconds(maxWaitSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("PatientId", patientId),
                Builders<BsonDocument>.Filter.Gt("TimestampUtc", afterUtc)
            );
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
        Guid patientId,
        DateTime afterUtc,
        int expectedCount = 1,
        int maxWaitSeconds = 15
    )
    {
        var deadline = DateTime.UtcNow.AddSeconds(maxWaitSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("PatientId", patientId),
                Builders<BsonDocument>.Filter.Gt("Timestamp", afterUtc)
            );
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

    public static object MalformedBatch(Guid patientId) =>
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
                    unit = "ThisISNotAUnit",
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
