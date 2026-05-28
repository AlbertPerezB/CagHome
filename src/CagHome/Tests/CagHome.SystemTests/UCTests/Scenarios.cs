using CagHome.Contracts.Enums;
using CagHome.SystemTests.Helpers;
using CagHome.SystemTests.TestClasses;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace CagHome.SystemTests.UCTests
{
    public class Scenarios : IClassFixture<AspireAppFixture>
    {
        private readonly TestHelpers _helpers;
        private readonly ITestOutputHelper _output;
        private static readonly TestPatient ActivePatient1 = TestPatient.ActiveCardiomyopathy();
        private static readonly TestPatient ActivePatient2 =
            TestPatient.ActiveCoronaryArteryDisease();

        public Scenarios(AspireAppFixture fixture, ITestOutputHelper output)
        {
            _helpers = new TestHelpers(fixture, output);
            _output = output;
        }

        [Fact]
        public async Task UC1_NormalMeasurement_EvaluatedWithNoEscalation()
        {
            var correlationId = await _helpers.InjectBatch(
                ActivePatient1.PatientId,
                TestHelpers.NormalBatch(ActivePatient1.PatientId)
            );
            //TODO: Check ingestion checkpoints?
            var audit = await _helpers.WaitForMonitoringAudit(correlationId);

            Assert.NotNull(audit);
            Assert.False(audit!["ShouldAlertPatient"].AsBoolean);
            Assert.False(audit["ShouldAlertHospital"].AsBoolean);
        }

        [Fact]
        public async Task UC2A_WarningMeasurement_PatientAlertOnly()
        {
            var correlationId = await _helpers.InjectBatch(
                ActivePatient1.PatientId,
                TestHelpers.WarningHeartRateBatch(ActivePatient1.PatientId)
            );
            var audit = await _helpers.WaitForMonitoringAudit(correlationId);

            Assert.NotNull(audit);
            Assert.True(audit!["ShouldAlertPatient"].AsBoolean);
            Assert.False(audit["ShouldAlertHospital"].AsBoolean);
            var severity = (Severity)audit["Severity"].AsInt32;
            Assert.Equal(Severity.Warning, severity);
            _output.WriteLine("UC2A — monitoring decision correct");

            var notifications = await _helpers.WaitForNotificationAudit(correlationId, 2);
            var delivered = notifications.FirstOrDefault(n =>
                n["Receiver"] == 1 && n["DeliveryStatus"] == 1
            );

            Assert.NotNull(delivered);
            _output.WriteLine("UC2A — patient notification delivered");

            // TODO: Check patient actually received
        }

        [Fact]
        public async Task UC2B_CriticalMeasurement_PatientAndHospitalAlert()
        {
            var correlationId = await _helpers.InjectBatch(
                ActivePatient2.PatientId,
                TestHelpers.CriticalHeartRateBatch(ActivePatient2.PatientId)
            );

            var audit = await _helpers.WaitForMonitoringAudit(correlationId);

            Assert.NotNull(audit);
            Assert.True(audit!["ShouldAlertPatient"].AsBoolean);
            Assert.True(audit["ShouldAlertHospital"].AsBoolean);
            var severity = (Severity)audit["Severity"].AsInt32;
            Assert.Equal(Severity.Critical, severity);
            _output.WriteLine("UC2B — monitoring decision correct");

            var notifications = await _helpers.WaitForNotificationAudit(correlationId, 4);

            var hospitalDelivered = notifications.FirstOrDefault(n =>
                n["Receiver"] == 0 && n["DeliveryStatus"] == 1
            //TODO: Check hospital actually received, and correct content (e.g. severity)
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

            //TODO: Check patient actually received
        }

        [Fact]
        public async Task UC3_ClinicianResponse_PickedUpAndDeliveredToPatient()
        {
            var beforeUtc = DateTime.UtcNow;
            var correlationId = Guid.NewGuid();
            var alertId = Guid.NewGuid();

            await _helpers.PostAlertToHospital(correlationId, alertId);

            await _helpers.PostClinicianResponse(
                alertId,
                ActivePatient1.PatientId,
                ActivePatient1.HospitalId,
                "Take 2mg adenosine and lay down"
            );

            var audits = await _helpers.WaitForNotificationAudit(
                correlationId,
                expectedCount: 2,
                maxWaitSeconds: 60
            );

            var delivered = audits.FirstOrDefault(a => a["DeliveryStatus"] == 1);
            Assert.NotNull(delivered);
            Assert.Contains("Take 2mg adenosine and lay down", delivered.ToString());
            _output.WriteLine("UC3 — clinician response processed and audit recorded");

            //TODO: Check if mesage delivered
        }

        [Fact]
        public async Task UC5_NewPatient_RegisteredViaEhr_AppearsInRegistryAndCache()
        {
            var newPatientId = Guid.NewGuid();

            await _helpers.RegisterPatientInMockEHR(
                newPatientId,
                careplan: (int)Careplan.ValveDisease,
                status: (int)PatientStatus.Active
            );

            var timeoutSeconds = 60;

            // Postcondition 1: Patient Registry has the new patient
            var registryEntry = await _helpers.WaitForPatientRegistry(
                newPatientId,
                maxWaitSeconds: timeoutSeconds
            );

            Assert.NotNull(registryEntry);
            _output.WriteLine($"UC5 — patient in registry: {registryEntry!["Status"]}");

            // Postcondition 2: Redis cache reflects the new patient
            var cachedStatus = await _helpers.WaitForRedisCache(
                newPatientId,
                maxWaitSeconds: timeoutSeconds
            );
            Assert.NotEmpty(cachedStatus);
            Assert.Equal("Active", cachedStatus);
            _output.WriteLine($"UC5 — patient cached in Redis as: {cachedStatus}");

            // Postcondition 3: Monitoring config store has the care plan
            var careplan = await _helpers.WaitForCareplansDb(
                newPatientId,
                maxWaitSeconds: timeoutSeconds
            );

            Assert.NotNull(careplan);
            _output.WriteLine($"UC5 — careplan stored: {careplan!["Careplan"]}");
        }
    }
}
