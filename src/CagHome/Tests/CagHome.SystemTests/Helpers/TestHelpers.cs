using System.Net.Http.Json;
using System.Text.Json;
using CagHome.Contracts;
using CagHome.Contracts.Enums;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQ.Client;
using Wolverine;
using Xunit.Abstractions;
using Xunit.Sdk;

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
    /// </summary>
    /// <param name="patientId"> The patient id to be injected</param>
    /// <param name="payload"> The payload to be injected <see cref = "NormalBatch(Guid)" /> </param>
    /// <returns>The correlationId to use for filtering audit entries.</returns>
    public async Task<Guid> InjectBatch(Guid patientId, object payload)
    {
        var response = await _fixture.Simulator.PostAsJsonAsync("/simulator/inject", payload);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<CorrelationIdResponse>();
        _output.WriteLine(
            $"Injected batch for patient {patientId} with correlation id {responseBody!.CorrelationId}"
        );
        return responseBody.CorrelationId;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="alertId"></param>
    /// <param name="patientId"></param>
    /// <param name="hospitalId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<Guid> PostClinicianResponse(
        Guid alertId,
        Guid patientId,
        Guid hospitalId,
        string message
    )
    {
        try
        {
            var payload = new
            {
                AlertId = alertId,
                CreatedAtUtc = DateTime.UtcNow,
                HospitalId = hospitalId,
                Message = message,
                PatientId = patientId,
            };

            var responseMessage = await _fixture.MockEhr.PostAsJsonAsync(
                "/mock/clinician-response",
                payload
            );
            responseMessage.EnsureSuccessStatusCode();
            _output.WriteLine($"Posted clinician response for alert {alertId}");
            var responseBody =
                await responseMessage.Content.ReadFromJsonAsync<CorrelationIdResponse>();
            return responseBody!.CorrelationId;
        }
        catch (Exception ex)
        {
            _output.WriteLine(
                $"Failed to post clinician response for alert {alertId}. "
                    + $"Exception: {ex.Message}"
            );
            return Guid.Empty;
        }
    }

    public async Task PostAlertToHospital(Guid correlationId, Guid alertId)
    {
        var alert = new HospitalAlertRequested(
            AlertId: alertId,
            CorrelationId: correlationId,
            DecidedAt: DateTime.UtcNow,
            HospitalId: Guid.NewGuid(),
            Message: "clincian response test",
            PatientId: Guid.NewGuid(),
            Severity: Severity.Critical
        );
        var responseMessage = await _fixture.MockEhr.PostAsJsonAsync("/alerts", alert);
        responseMessage.EnsureSuccessStatusCode();
        _output.WriteLine($"Posted hospital alert for alert {alert.AlertId}");
    }

    public async Task RegisterPatientInMockEHR(
        Guid patientId,
        int careplan,
        int status,
        string? timestamp = null
    )
    {
        var payload = new
        {
            PatientId = patientId,
            UpdatedAtUtc = timestamp ?? DateTime.UtcNow.ToString("O"),
            Careplan = careplan,
            Status = status,
        };
        var responseMessage = await _fixture.MockEhr.PostAsJsonAsync("/mock/patient", payload);
        responseMessage.EnsureSuccessStatusCode();
        _output.WriteLine($"Registered patient {patientId} in mock EHR");
    }

    public async Task<BsonDocument?> WaitForMonitoringAudit(
        Guid correlationId,
        int maxWaitSeconds = 30
    )
    {
        return await PollUntilAsync(
            async () =>
            {
                var filter = Builders<BsonDocument>.Filter.Eq("CorrelationId", correlationId);
                return await _fixture
                    .MonitoringAudit.Find(filter)
                    .SortByDescending(d => d["TimestampUtc"])
                    .FirstOrDefaultAsync();
            },
            TimeSpan.FromSeconds(maxWaitSeconds)
        );
    }

    public async Task<List<BsonDocument>> WaitForNotificationAudit(
        Guid correlationId,
        int expectedCount = 1,
        int maxWaitSeconds = 15
    )
    {
        var result = await PollUntilAsync(
            async () =>
            {
                var filter = Builders<BsonDocument>.Filter.Eq("CorrelationId", correlationId);
                var entries = await _fixture.NotificationAudit.Find(filter).ToListAsync();
                return entries.Count >= expectedCount ? entries : null;
            },
            TimeSpan.FromSeconds(maxWaitSeconds)
        );

        return result ?? new List<BsonDocument>();
    }

    public async Task<BsonDocument?> WaitForCareplansDb(Guid patientId, int maxWaitSeconds = 15)
    {
        return await PollUntilAsync(
            async () =>
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", patientId);
                return await _fixture.MonitoringCareplans.Find(filter).FirstOrDefaultAsync();
            },
            TimeSpan.FromSeconds(maxWaitSeconds)
        );
    }

    public async Task<BsonDocument?> WaitForPatientRegistry(Guid patientId, int maxWaitSeconds = 15)
    {
        return await PollUntilAsync(
            async () =>
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", patientId);
                return await _fixture.PatientRegistry.Find(filter).FirstOrDefaultAsync();
            },
            TimeSpan.FromSeconds(maxWaitSeconds)
        );
    }

    public async Task<string> WaitForRedisCache(Guid patientId, int maxWaitSeconds = 15)
    {
        return await PollUntilAsync(
                async () =>
                {
                    var val = await _fixture.RedisCache.StringGetAsync(
                        $"patient:{patientId}:status"
                    );
                    return val.IsNullOrEmpty ? null : val.ToString();
                },
                TimeSpan.FromSeconds(maxWaitSeconds)
            ) ?? string.Empty;
    }

    private static async Task<T?> PollUntilAsync<T>(Func<Task<T?>> probe, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var result = await probe();
            if (result is not null)
                return result;
            await Task.Delay(1000);
        }
        return default;
    }

    public async Task PublishStalePatientUpdate(
        Guid patientId,
        PatientStatus status,
        DateTime staleTimestamp
    )
    {
        var bus = await _fixture.GetTestMessageBus();
        await bus.PublishAsync(new PatientStatusUpdateRequested(patientId, status, staleTimestamp));
        _output.WriteLine(
            $"Published stale update for {patientId} with timestamp {staleTimestamp:O}"
        );
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

public record CorrelationIdResponse(Guid CorrelationId);
