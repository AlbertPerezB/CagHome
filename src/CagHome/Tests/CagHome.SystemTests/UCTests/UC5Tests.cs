using CagHome.Contracts.Enums;
using CagHome.SystemTests.Helpers;
using CagHome.SystemTests.TestClasses;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace CagHome.SystemTests.UCTests
{
    public class UC5Tests : IClassFixture<AspireAppFixture>
    {
        private readonly TestHelpers _helpers;
        private readonly AspireAppFixture _fixture;
        private readonly ITestOutputHelper _output;
        private static readonly TestPatient ActivePatient = TestPatient.ActiveCardiomyopathy();
        private static readonly TestPatient DeceasedPatient = TestPatient.DeceasedValveDisease();

        public UC5Tests(AspireAppFixture fixture, ITestOutputHelper output)
        {
            _helpers = new TestHelpers(fixture, output);
            _output = output;
            _fixture = fixture;
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

            var timeout = TimeSpan.FromSeconds(60);

            // Postcondition 1: Patient Registry has the new patient
            var registryEntry = await PollUntilAsync(
                async () =>
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("_id", newPatientId);
                    return await _fixture.PatientRegistry.Find(filter).FirstOrDefaultAsync();
                },
                timeout
            );

            Assert.NotNull(registryEntry);
            _output.WriteLine($"UC5 — patient in registry: {registryEntry!["Status"]}");

            // Postcondition 2: Redis cache reflects the new patient
            var cachedStatus = await PollUntilAsync(
                async () =>
                {
                    var val = await _fixture.RedisCache.StringGetAsync(
                        $"patient:{newPatientId}:status"
                    );
                    return val.IsNullOrEmpty ? null : val.ToString();
                },
                timeout
            );

            Assert.NotNull(cachedStatus);
            Assert.Equal("Active", cachedStatus);
            _output.WriteLine($"UC5 — patient cached in Redis as: {cachedStatus}");

            // Postcondition 3: Monitoring config store has the care plan
            var careplan = await PollUntilAsync(
                async () =>
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("_id", newPatientId);
                    return await _fixture.MonitoringCareplans.Find(filter).FirstOrDefaultAsync();
                },
                timeout
            );

            Assert.NotNull(careplan);
            _output.WriteLine($"UC5 — careplan stored: {careplan!["Careplan"]}");
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
    }
}
