using CagHome.Contracts.Enums;
using CagHome.IngestionService.Application.Validation.BatchValidation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Cache;
using NSubstitute;

namespace CagHome.IngestionService.Tests.UnitTests;

public class PatientActiveRuleTests
{
    private readonly IPatientRegistryCache _cache;
    private readonly PatientActiveRule _rule;

    public PatientActiveRuleTests()
    {
        _cache = Substitute.For<IPatientRegistryCache>();
        _rule = new PatientActiveRule(_cache);
    }

    [Fact]
    public void IsFatal_ShouldBeTrue()
    {
        Assert.True(_rule.IsFatal);
    }

    [Fact]
    public async Task Validate_WhenPatientActive_ShouldReturnNull()
    {
        var patientId = Guid.Parse("d9aaf610-c81e-4dd7-8e1e-3fa6c4cf9c18");
        _cache.GetPatientStatus(patientId).Returns(PatientStatus.Active);

        var batch = TestDataFactory.MakeBatch(patientId: patientId);
        var result = await _rule.ValidateAsync(batch);

        Assert.Null(result);
    }

    [Fact]
    public async Task Validate_WhenPatientInactive_ShouldReturnPatientInactiveError()
    {
        var patientId = Guid.NewGuid();
        _cache.GetPatientStatus(patientId).Returns(PatientStatus.Inactive);

        var batch = TestDataFactory.MakeBatch(patientId: patientId);
        var result = await _rule.ValidateAsync(batch);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.PatientInactive, result!.Code);
        Assert.Contains(patientId.ToString(), result.Message);
    }

    [Fact]
    public async Task Validate_WhenPatientNotInCache_ShouldReturnPatientNotEnrolledError()
    {
        var patientId = Guid.NewGuid();
        _cache.GetPatientStatus(patientId).Returns((PatientStatus?)null);

        var batch = TestDataFactory.MakeBatch(patientId: patientId);
        var result = await _rule.ValidateAsync(batch);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.PatientNotEnrolled, result!.Code);
        Assert.Contains(patientId.ToString(), result.Message);
    }

    [Fact]
    public async Task Validate_WhenPatientDeceased_ShouldReturnError_ButCurrentlyDoesNot()
    {
        var patientId = Guid.Parse("b2ffdfe8-47ef-42c3-9a7a-94fc3cea8f34");
        _cache.GetPatientStatus(patientId).Returns(PatientStatus.Deceased);

        var batch = TestDataFactory.MakeBatch(patientId: patientId);
        var result = await _rule.ValidateAsync(batch);
        Assert.NotNull(result);
    }
}
