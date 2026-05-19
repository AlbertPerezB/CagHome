using System.Text;
using CagHome.SystemTests.Helpers;
using Xunit.Abstractions;

namespace CagHome.SystemTests.UCTests
{
    public class UC4Tests : IClassFixture<AspireAppFixture>
    {
        private readonly TestHelpers _helpers;
        private readonly AspireAppFixture _fixture;
        private readonly ITestOutputHelper _output;

        public UC4Tests(AspireAppFixture fixture, ITestOutputHelper output)
        {
            _helpers = new TestHelpers(fixture, output);
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task UC4A_MalformedJson_NeverReachesMonitoring()
        {
            // Count audit entries before
            var auditCountBefore = await _helpers.GetMonitoringAuditCount();

            // Post malformed JSON and assert 400 Bad Request
            var response = await _fixture.Simulator.PostAsync(
                "/simulator/inject",
                new StringContent("{ not valid json }", Encoding.UTF8, "application/json")
            );
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await Task.Delay(15000);

            // Count audit entries after
            var auditCountAfter = await _helpers.GetMonitoringAuditCount();
            Assert.Equal(auditCountBefore, auditCountAfter);

            _output.WriteLine("UC4A — malformed JSON did not reach monitoring and returned 400");
        }
    }
}
