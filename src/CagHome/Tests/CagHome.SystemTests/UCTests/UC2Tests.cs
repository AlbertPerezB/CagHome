using CagHome.Contracts.Enums;
using CagHome.SystemTests.Helpers;
using CagHome.SystemTests.TestClasses;
using Xunit.Abstractions;

namespace CagHome.SystemTests.UCTests
{
    public class UC2Tests : IClassFixture<AspireAppFixture>
    {
        private readonly TestHelpers _helpers;
        private readonly ITestOutputHelper _output;
        private static readonly TestPatient ActivePatient1 = TestPatient.ActiveCardiomyopathy();
        private static readonly TestPatient ActivePatient2 =
            TestPatient.ActiveCoronaryArteryDisease();

        public UC2Tests(AspireAppFixture fixture, ITestOutputHelper output)
        {
            _helpers = new TestHelpers(fixture, output);
            _output = output;
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

            var notifications = await _helpers.WaitForNotificationAudit(
                correlationId,
                2
            );
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

            var notifications = await _helpers.WaitForNotificationAudit(
                correlationId,
                3
            );

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
    }
}
