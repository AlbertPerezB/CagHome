using CagHome.SystemTests.Helpers;
using CagHome.SystemTests.TestClasses;
using Xunit.Abstractions;

namespace CagHome.SystemTests.UCTests;

public class UC3Tests : IClassFixture<AspireAppFixture>
{
    private readonly TestHelpers _helpers;
    private readonly ITestOutputHelper _output;
    private static readonly TestPatient ActivePatient = TestPatient.ActiveCardiomyopathy();

    public UC3Tests(AspireAppFixture fixture, ITestOutputHelper output)
    {
        _helpers = new TestHelpers(fixture, output);
        _output = output;
    }

    [Fact]
    public async Task UC3_ClinicianResponse_PickedUpAndDeliveredToPatient()
    {
        var beforeUtc = DateTime.UtcNow;
        var alertId = Guid.NewGuid();

        await _helpers.PostClinicianResponse(
            alertId,
            ActivePatient.PatientId,
            ActivePatient.HospitalId,
            "Take 2mg adenosine and lay down"
        );
        _output.WriteLine("Clinician response posted, waiting for notification audit...");

        var audits = await _helpers.WaitForNotificationAudit(
            ActivePatient.PatientId,
            beforeUtc,
            expectedCount: 2
        );

        var delivered = audits.FirstOrDefault(a => a["DeliveryStatus"] == 1);
        Assert.NotNull(delivered);
        _output.WriteLine("UC3 — clinician response processed and audit recorded");

        //TODO: Check if mesage delivered
    }
}
