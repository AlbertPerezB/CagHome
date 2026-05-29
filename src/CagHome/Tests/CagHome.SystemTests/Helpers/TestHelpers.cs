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

    /// <summary>
    /// Posts an alert directly to the hospital endpoint, bypassing the monitoring and notification service.
    /// </summary>
    /// <param name="correlationId">The correlation ID for the alert.</param>
    /// <param name="alertId">The alert ID.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

    /// <summary>
    /// Registers a patient in the mock EHR system with the specified care plan and status.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient to register.</param>
    /// <param name="careplan">The care plan code to associate with the patient.</param>
    /// <param name="status">The status code representing the patient's current state in the EHR.</param>
    /// <param name="timestamp">The UTC timestamp to record as the update time. If null, the current UTC time is used. </param>
    /// <returns>A task that represents the asynchronous registration operation.</returns>
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

    /// <summary>
    /// Waits asynchronously for a monitoring audit document with the specified correlation ID to become available, or
    /// until the maximum wait time elapses.
    /// </summary>
    /// <param name="correlationId">The unique identifier used to locate the monitoring audit document.</param>
    /// <param name="maxWaitSeconds">The maximum number of seconds to wait for the monitoring audit document to appear.
    /// The default is 30 seconds.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the most recent monitoring audit
    /// document matching the specified correlation ID, or null if no such document is found within the wait period.</returns>
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

    /// <summary>
    /// Waits asynchronously until the specified number of notification audit entries with the given correlation ID are
    /// available, or until the maximum wait time elapses.
    /// </summary>
    /// <param name="correlationId">The correlation identifier used to filter notification audit entries.</param>
    /// <param name="expectedCount">The minimum number of notification audit entries to wait for. Must be greater than or equal to 1. The method
    /// returns as soon as this number of entries is found.</param>
    /// <param name="maxWaitSeconds">The maximum number of seconds to wait for the expected entries before returning. Must be greater than 0.</param>
    /// <returns>A list of <see cref="BsonDocument"/> objects representing the notification audit entries that match the
    /// specified correlation ID. Returns an empty list if the expected number of entries is not found within the
    /// maximum wait time.</returns>
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

    /// <summary>
    /// Waits asynchronously for a care plan document associated with the specified patient to become available in the
    /// database, or until the maximum wait time elapses.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient whose care plan document is being queried.</param>
    /// <param name="maxWaitSeconds">The maximum number of seconds to wait for the care plan document to appear in the database. Must be a positive
    /// integer. The default is 15 seconds.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the care plan document for the
    /// specified patient if found within the wait period; otherwise, null.</returns>
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

    /// <summary>
    /// Waits asynchronously for a patient registry document with the specified patient identifier to become available,
    /// or until the maximum wait time elapses.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient whose registry document to retrieve.</param>
    /// <param name="maxWaitSeconds">The maximum number of seconds to wait for the patient registry document to become available. Must be a positive
    /// integer. The default is 15 seconds.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the patient registry document if
    /// found within the specified time; otherwise, null.</returns>
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

    /// <summary>
    /// Waits asynchronously for a status value to become available in the Redis cache for the specified patient, or
    /// until the maximum wait time elapses.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient whose status is being polled in the Redis cache.</param>
    /// <param name="maxWaitSeconds">The maximum number of seconds to wait for the status value to appear in the cache.
    /// The default is 15 seconds.</param>
    /// <returns>A string containing the status value from the Redis cache if available within the wait period; otherwise, an
    /// empty string.</returns>
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

    /// <summary>
    /// Publishes a patient update event directly to the RabbitMQ message bus bypassing the EHR integration service's pollers
    /// to simulate a stale patient status scenario, allowing tests to verify the system's handling of outdated information.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient whose status update is being published.</param>
    /// <param name="status">The status to associate with the patient in the update message.</param>
    /// <param name="staleTimestamp">The timestamp indicating when the patient's status became stale.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
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

    /// <summary>
    /// Creates a sample batch payload containing heart rate, SpO2, and body temperature measurements for the specified
    /// patient.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient for whom the measurement batch is generated.</param>
    /// <returns>An anonymous object representing a batch of measurements for the specified patient, including schema and
    /// application version information, patient ID, and an array of measurement records.</returns>
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

    /// <summary>
    /// Generates a batch of warning heart rate and SpO2 measurement data for the specified patient.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient for whom the measurement batch is generated.</param>
    /// <returns>An object containing schema and application version information, the patient identifier, and a collection of
    /// simulated heart rate and SpO2 measurements.</returns>
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

    /// <summary>
    /// Generates a sample batch of critical heart rate and SpO2 measurements for the specified patient.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient for whom the measurement batch is generated.</param>
    /// <returns>An anonymous object containing schema and application version information, the patient identifier, and a
    /// collection of sample measurement records representing critical heart rate and SpO2 values.</returns>
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

/// <summary>
/// Represents a response containing a correlation identifier used to track or associate related operations.
/// </summary>
/// <param name="CorrelationId">The unique identifier assigned to correlate related requests or operations.</param>
public record CorrelationIdResponse(Guid CorrelationId);
