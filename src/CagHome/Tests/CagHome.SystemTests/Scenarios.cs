using MongoDB.Driver;
using Xunit.Abstractions;

namespace CagHome.SystemTests;

public class EndToEndScenarioTests : IClassFixture<AspireAppFixture>
{
    private readonly AspireAppFixture _fixture;
    private readonly TestHelpers _helpers;
    private readonly ITestOutputHelper _output;

    private static readonly Guid ActivePatient = Guid.Parse("d9aaf610-c81e-4dd7-8e1e-3fa6c4cf9c18");
    private static readonly Guid DeceasedPatient = Guid.Parse(
        "b2ffdfe8-47ef-42c3-9a7a-94fc3cea8f34"
    );
    private static readonly TimeSpan PipelineTimeout = TimeSpan.FromSeconds(15);

    public EndToEndScenarioTests(AspireAppFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _helpers = new TestHelpers(fixture, output);
    }

    // ═══════════════════════════════════════════════════════
    //  UC1: Normal measurement — no escalation
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC1_NormalMeasurement_EvaluatedWithNoEscalation()
    {
        var correlationId = await _helpers.InjectBatch(
            ActivePatient,
            TestHelpers.NormalBatch(ActivePatient)
        );

        var audit = await _helpers.WaitForMonitoringAudit(correlationId);

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
        var correlationId = await _helpers.InjectBatch(
            ActivePatient,
            TestHelpers.WarningHeartRateBatch(ActivePatient)
        );

        var audit = await _helpers.WaitForMonitoringAudit(correlationId);

        Assert.NotNull(audit);
        Assert.True(audit!["ShouldAlertPatient"].AsBoolean);
        Assert.False(audit["ShouldAlertHospital"].AsBoolean);
        Assert.Equal("Warning", audit["Severity"].AsString);
        _output.WriteLine("UC2A — monitoring decision correct");

        var notifications = await _helpers.WaitForNotificationAudit(
            correlationId,
            expectedCount: 2
        );
        var delivered = notifications.FirstOrDefault(n =>
            n["Receiver"].AsString == "Patient" && n["DeliveryStatus"].AsString == "Delivered"
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
        var correlationId = await _helpers.InjectBatch(
            ActivePatient,
            TestHelpers.CriticalHeartRateBatch(ActivePatient)
        );

        var audit = await _helpers.WaitForMonitoringAudit(correlationId);

        Assert.NotNull(audit);
        Assert.True(audit!["ShouldAlertPatient"].AsBoolean);
        Assert.True(audit["ShouldAlertHospital"].AsBoolean);
        Assert.Equal("Critical", audit["Severity"].AsString);
        _output.WriteLine("UC2B — monitoring decision correct");

        var notifications = await _helpers.WaitForNotificationAudit(
            correlationId,
            expectedCount: 3
        );

        var hospitalDelivered = notifications.FirstOrDefault(n =>
            n["Receiver"].AsString == "Hospital" && n["DeliveryStatus"].AsString == "Delivered"
        );
        Assert.NotNull(hospitalDelivered);
        _output.WriteLine(
            $"UC2B — hospital alert delivered, status: {hospitalDelivered!["StatusCode"]}"
        );

        var patientDelivered = notifications.FirstOrDefault(n =>
            n["Receiver"].AsString == "Patient" && n["DeliveryStatus"].AsString == "Delivered"
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
        // Malformed JSON won't get a proper correlation ID from the endpoint,
        // so we fall back to timestamp-based absence check
        var beforeUtc = DateTime.UtcNow;

        await _fixture.Simulator.PostAsync(
            "/simulator/inject",
            new StringContent("{ not valid json }", System.Text.Encoding.UTF8, "application/json")
        );

        // Wait for pipeline to process (or not), then verify nothing arrived
        await Task.Delay(PipelineTimeout);

        var collection = _fixture.MonitoringAuditDb.GetCollection<MongoDB.Bson.BsonDocument>(
            "DecisionAuditEntries"
        );
        var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.And(
            MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq(
                "PatientId",
                ActivePatient
            ),
            MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Gt("TimestampUtc", beforeUtc)
        );
        var entry = await collection.Find(filter).FirstOrDefaultAsync();

        Assert.Null(entry);
        _output.WriteLine("UC4A — malformed JSON did not reach monitoring");
    }

    // ═══════════════════════════════════════════════════════
    //  UC5: Cache warm-up — Redis reflects patient registry
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC5_ActivePatient_PresentInRedisCache()
    {
        var db = _fixture.Redis.GetDatabase();
        var status = await db.StringGetAsync($"patient:{ActivePatient}:status");

        Assert.False(status.IsNullOrEmpty);
        Assert.Equal("Active", status.ToString());
        _output.WriteLine($"UC5 — active patient cached as: {status}");
    }

    [Fact]
    public async Task UC5_DeceasedPatient_PresentInRedisCache()
    {
        var db = _fixture.Redis.GetDatabase();
        var status = await db.StringGetAsync($"patient:{DeceasedPatient}:status");

        Assert.False(status.IsNullOrEmpty);
        _output.WriteLine($"UC5 — deceased patient cached as: {status}");
    }

    // ═══════════════════════════════════════════════════════
    //  UC5: Patient registry — MongoDB has patient data
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task UC5_ActivePatient_ExistsInPatientRegistry()
    {
        var collection = _fixture.PatientRegistryDb.GetCollection<MongoDB.Bson.BsonDocument>(
            "PatientData"
        );
        var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq(
            "PatientId",
            ActivePatient
        );
        var patient = await collection.Find(filter).FirstOrDefaultAsync();

        Assert.NotNull(patient);
        _output.WriteLine($"UC5 — active patient found in registry: {patient["Status"]}");
    }
}
