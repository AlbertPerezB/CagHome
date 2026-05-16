using System.Net.Http.Json;
using CagHome.Contracts.Enums;
using MongoDB.Bson;
using MongoDB.Driver;
using MQTTnet;
using Xunit.Abstractions;

namespace CagHome.SystemTests;

public class EndToEndScenarioTests : IClassFixture<AspireAppFixture>, IAsyncLifetime
{
    private readonly AspireAppFixture _fixture;
    private readonly TestHelpers _helpers;
    private readonly ITestOutputHelper _output;

    private static readonly TestPatient ActivePatient = TestPatient.ActiveCardiomyopathy();
    private static readonly TestPatient InactivePatient =
        TestPatient.InactiveCoronaryArteryDisease();
    private static readonly TestPatient DeceasedPatient = TestPatient.DeceasedValveDisease();

    private static readonly TimeSpan PipelineTimeout = TimeSpan.FromSeconds(15);

    public EndToEndScenarioTests(AspireAppFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _helpers = new TestHelpers(fixture, output);
    }

    public async Task InitializeAsync()
    {
        foreach (var patient in TestPatient.All())
        {
            await _fixture.SeedPatient(patient);
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ═══════════════════════════════════════════════════════
    //  UC1: Normal measurement — no escalation
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC1_NormalMeasurement_EvaluatedWithNoEscalation()
    {
        var beforeUtc = await _helpers.InjectBatch(
            ActivePatient.PatientId,
            TestHelpers.NormalBatch(ActivePatient.PatientId)
        );

        var audit = await _helpers.WaitForMonitoringAudit(ActivePatient.PatientId, beforeUtc);

        Assert.NotNull(audit);
        Assert.False(audit!["ShouldAlertPatient"].AsBoolean);
        Assert.False(audit["ShouldAlertHospital"].AsBoolean);
        _output.WriteLine($"UC1 passed — no escalation, policy: {audit["PolicyName"]}");
    }

    // ═══════════════════════════════════════════════════════
    //  UC2 Path A: Warning — patient alert only
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC2A_WarningMeasurement_PatientAlertOnly()
    {
        var beforeUtc = await _helpers.InjectBatch(
            ActivePatient.PatientId,
            TestHelpers.WarningHeartRateBatch(ActivePatient.PatientId)
        );

        var audit = await _helpers.WaitForMonitoringAudit(ActivePatient.PatientId, beforeUtc);

        Assert.NotNull(audit);
        Assert.True(audit!["ShouldAlertPatient"].AsBoolean);
        Assert.False(audit["ShouldAlertHospital"].AsBoolean);
        var severity = (Severity)audit["Severity"].AsInt32;
        Assert.Equal(Severity.Warning, severity);
        _output.WriteLine("UC2A — monitoring decision correct");

        var notifications = await _helpers.WaitForNotificationAudit(
            ActivePatient.PatientId,
            beforeUtc,
            expectedCount: 2
        );
        var delivered = notifications.FirstOrDefault(n =>
            n["Receiver"] == 1 && n["DeliveryStatus"] == 1
        );

        Assert.NotNull(delivered);
        _output.WriteLine("UC2A — patient notification delivered");
    }

    // ═══════════════════════════════════════════════════════
    //  UC2 Path B: Critical — patient AND hospital alert
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC2B_CriticalMeasurement_PatientAndHospitalAlert()
    {
        var beforeUtc = await _helpers.InjectBatch(
            ActivePatient.PatientId,
            TestHelpers.CriticalHeartRateBatch(ActivePatient.PatientId)
        );

        var audit = await _helpers.WaitForMonitoringAudit(ActivePatient.PatientId, beforeUtc);

        Assert.NotNull(audit);
        Assert.True(audit!["ShouldAlertPatient"].AsBoolean);
        Assert.True(audit["ShouldAlertHospital"].AsBoolean);
        var severity = (Severity)audit["Severity"].AsInt32;
        Assert.Equal(Severity.Critical, severity);
        _output.WriteLine("UC2B — monitoring decision correct");

        var notifications = await _helpers.WaitForNotificationAudit(
            ActivePatient.PatientId,
            beforeUtc,
            expectedCount: 3
        );

        var hospitalDelivered = notifications.FirstOrDefault(n =>
            n["Receiver"] == 0 && n["DeliveryStatus"] == 1
        );
        Assert.NotNull(hospitalDelivered);
        _output.WriteLine(
            $"UC2B — hospital alert delivered, status: {hospitalDelivered!["StatusCode"]}"
        );

        var patientDelivered = notifications.FirstOrDefault(n =>
            n["Receiver"] == 1 && n["DeliveryStatus"] == 1
        );
        Assert.NotNull(patientDelivered);
        _output.WriteLine("UC2B — patient notification also delivered");
    }

    // ═══════════════════════════════════════════════════════
    //  UC4 Path A: Malformed JSON — rejected, no evaluation
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC4A_MalformedJson_NeverReachesMonitoring()
    {
        var beforeUtc = DateTime.UtcNow;

        await _fixture.Simulator.PostAsync(
            "/simulator/inject",
            new StringContent("{ not valid json }", System.Text.Encoding.UTF8, "application/json")
        );

        await Task.Delay(PipelineTimeout);

        var audit = await _helpers.WaitForMonitoringAudit(
            ActivePatient.PatientId,
            beforeUtc,
            maxWaitSeconds: 3
        );

        Assert.Null(audit);
        _output.WriteLine("UC4A — malformed JSON did not reach monitoring");
    }

    // ═══════════════════════════════════════════════════════
    //  UC5: Cache warm-up — Redis reflects seeded patients
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC5_ActivePatient_PresentInRedisCache()
    {
        var status = await _fixture.RedisCache.StringGetAsync(
            $"patient:{ActivePatient.PatientId}:status"
        );

        Assert.False(status.IsNullOrEmpty);
        Assert.Equal("Active", status.ToString());
        _output.WriteLine($"UC5 — active patient cached as: {status}");
    }

    [Fact]
    public async Task UC5_DeceasedPatient_PresentInRedisCache()
    {
        var status = await _fixture.RedisCache.StringGetAsync(
            $"patient:{DeceasedPatient.PatientId}:status"
        );

        Assert.False(status.IsNullOrEmpty);
        Assert.Equal("Deceased", status.ToString());
        _output.WriteLine($"UC5 — deceased patient cached as: {status}");
    }

    // ═══════════════════════════════════════════════════════
    //  UC5: Patient registry — MongoDB has patient data
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC5_ActivePatient_ExistsInPatientRegistry()
    {
        var filter = Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", ActivePatient.PatientId);
        var patient = await _fixture.PatientRegistry.Find(filter).FirstOrDefaultAsync();

        Assert.NotNull(patient);
        _output.WriteLine($"UC5 — active patient found in registry: {patient["Status"]}");
    }

    // ═══════════════════════════════════════════════════════
    //  UC5: New patient registration via Mock EHR
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC5_NewPatient_RegisteredViaEhr_AppearsInRegistryAndCache()
    {
        var newPatientId = Guid.NewGuid();

        var response = await _fixture.MockEhr.PostAsJsonAsync(
            "/mock/patient",
            new
            {
                PatientId = newPatientId,
                UpdatedAtUtc = DateTime.UtcNow.ToString("O"),
                Careplan = 1,
                Status = 0,
            }
        );
        response.EnsureSuccessStatusCode();
        _output.WriteLine($"Posted new patient {newPatientId} to Mock EHR");

        // Wait for EHR Integration polling → Patient Registry → Redis
        var deadline = DateTime.UtcNow.AddSeconds(60);
        MongoDB.Bson.BsonDocument? registryEntry = null;

        while (DateTime.UtcNow < deadline)
        {
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq(
                "_id",
                newPatientId
            );
            registryEntry = await _fixture.PatientRegistry.Find(filter).FirstOrDefaultAsync();
            if (registryEntry != null)
                break;
            await Task.Delay(1000);
        }

        Assert.NotNull(registryEntry);
        _output.WriteLine($"New patient found in registry: {registryEntry!["Status"]}");

        var cachedStatus = await _fixture.RedisCache.StringGetAsync(
            $"patient:{newPatientId}:status"
        );
        Assert.False(cachedStatus.IsNullOrEmpty);
        Assert.Equal("Active", cachedStatus.ToString());
        _output.WriteLine($"New patient cached in Redis as: {cachedStatus}");
    }
}
