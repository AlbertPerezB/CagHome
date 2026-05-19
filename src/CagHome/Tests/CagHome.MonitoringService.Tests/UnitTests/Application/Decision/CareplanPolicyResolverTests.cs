using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision;
using CagHome.MonitoringService.Tests.Helpers;

namespace CagHome.MonitoringService.Tests.UnitTests.Application.Decision;

public class CareplanPolicyResolverTests
{
    [Fact]
    public void Resolve_WithExactPolicy_ReturnsExactPolicy()
    {
        var expectedPolicy = new TestPolicy(Careplan.ValveDisease);
        var resolver = new CareplanPolicyResolver([new TestPolicy(Careplan.None), expectedPolicy]);

        var policy = resolver.Resolve(Careplan.ValveDisease);

        Assert.Same(expectedPolicy, policy);
    }

    [Fact]
    public void Resolve_WhenSpecificMissing_ReturnsNoneFallbackPolicy()
    {
        var fallback = new TestPolicy(Careplan.None);
        var resolver = new CareplanPolicyResolver([fallback]);

        var policy = resolver.Resolve(Careplan.Cardiomyopathy);

        Assert.Same(fallback, policy);
    }

    [Fact]
    public void Constructor_WithDuplicateCareplanPolicies_Throws()
    {
        var duplicatePolicyA = new TestPolicy(Careplan.CoronaryArteryDisease);
        var duplicatePolicyB = new TestPolicy(Careplan.CoronaryArteryDisease);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new CareplanPolicyResolver([duplicatePolicyA, duplicatePolicyB, new TestPolicy(Careplan.None)])
        );

        Assert.Contains(Careplan.CoronaryArteryDisease.ToString(), exception.Message);
    }

    [Fact]
    public void Resolve_WhenSpecificAndFallbackMissing_Throws()
    {
        var resolver = new CareplanPolicyResolver([new TestPolicy(Careplan.ValveDisease)]);

        var exception = Assert.Throws<KeyNotFoundException>(() => resolver.Resolve(Careplan.None));

        Assert.Contains(Careplan.None.ToString(), exception.Message);
    }
}