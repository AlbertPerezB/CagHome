using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.IngestionService.Infrastructure.Cache;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CagHome.IngestionService.Tests.UnitTests;

public class PatientStatusUpdatedConsumerTests
{
    private readonly IPatientRegistryCache _cache;
    private readonly ILogger<PatientStatusUpdatedConsumer> _logger;

    public PatientStatusUpdatedConsumerTests()
    {
        _cache = Substitute.For<IPatientRegistryCache>();
        _logger = Substitute.For<ILogger<PatientStatusUpdatedConsumer>>();
    }

    [Fact]
    public async Task Handle_ShouldUpdateCacheWithCorrectPatientAndStatus()
    {
        var patientId = Guid.NewGuid();
        var message = new PatientStatusUpdated(patientId, PatientStatus.Active, DateTime.UtcNow);

        await PatientStatusUpdatedConsumer.Handle(message, _cache, _logger);

        await _cache.Received(1).SetPatientStatus(patientId, PatientStatus.Active);
    }

    [Fact]
    public async Task Handle_WhenPatientDeactivated_ShouldSetInactive()
    {
        var patientId = Guid.NewGuid();
        var message = new PatientStatusUpdated(patientId, PatientStatus.Inactive, DateTime.UtcNow);

        await PatientStatusUpdatedConsumer.Handle(message, _cache, _logger);

        await _cache.Received(1).SetPatientStatus(patientId, PatientStatus.Inactive);
    }

    [Fact]
    public async Task Handle_WhenPatientDeceased_ShouldSetDeceased()
    {
        var patientId = Guid.Parse("b2ffdfe8-47ef-42c3-9a7a-94fc3cea8f34");
        var message = new PatientStatusUpdated(patientId, PatientStatus.Deceased, DateTime.UtcNow);

        await PatientStatusUpdatedConsumer.Handle(message, _cache, _logger);

        await _cache.Received(1).SetPatientStatus(patientId, PatientStatus.Deceased);
    }

    [Fact]
    public async Task Handle_WhenCacheThrows_ShouldPropagate()
    {
        var message = new PatientStatusUpdated(
            Guid.NewGuid(),
            PatientStatus.Active,
            DateTime.UtcNow
        );
        _cache
            .SetPatientStatus(Arg.Any<Guid>(), Arg.Any<PatientStatus>())
            .ThrowsAsync(new Exception("Redis unreachable"));

        await Assert.ThrowsAsync<Exception>(() =>
            PatientStatusUpdatedConsumer.Handle(message, _cache, _logger)
        );
    }
}
