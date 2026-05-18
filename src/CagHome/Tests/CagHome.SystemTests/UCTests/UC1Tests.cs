using CagHome.SystemTests.Helpers;
using CagHome.SystemTests.TestClasses;
using Xunit.Abstractions;

namespace CagHome.SystemTests.UCTests
{
    public class UC1Tests : IClassFixture<AspireAppFixture>
    {
        private readonly TestHelpers _helpers;
        private static readonly TestPatient ActivePatient = TestPatient.ActiveCardiomyopathy();

        public UC1Tests(AspireAppFixture fixture, ITestOutputHelper output)
        {
            _helpers = new TestHelpers(fixture, output);
        }

        [Fact]
        public async Task UC1_NormalMeasurement_EvaluatedWithNoEscalation()
        {
            var correlationId = await _helpers.InjectBatch(
                ActivePatient.PatientId,
                TestHelpers.NormalBatch(ActivePatient.PatientId)
            );
            //TODO: Check ingestion checkpoints?
            var audit = await _helpers.WaitForMonitoringAudit(correlationId);

            Assert.NotNull(audit);
            Assert.False(audit!["ShouldAlertPatient"].AsBoolean);
            Assert.False(audit["ShouldAlertHospital"].AsBoolean);
        }
    }
}
