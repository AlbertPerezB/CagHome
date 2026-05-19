using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision;

namespace CagHome.MonitoringService.Tests.UnitTests.Application.Decision;

public class CooldownServiceTests
{
    [Fact]
    public void Evaluate_WarningWithinWindow_SuppressesSecondAlert()
    {
        var service = new CooldownService();
        var patientId = Guid.NewGuid();
        var first = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var second = first.AddMinutes(10);

        var firstResult = service.Evaluate(patientId, Severity.Warning, first);
        var secondResult = service.Evaluate(patientId, Severity.Warning, second);

        Assert.False(firstResult.IsSuppressed);
        Assert.True(secondResult.IsSuppressed);
        Assert.Equal(TimeSpan.FromMinutes(5), secondResult.RemainingCooldown);
    }

    [Fact]
    public void Evaluate_CriticalAfterWindow_DoesNotSuppressSecondAlert()
    {
        var service = new CooldownService();
        var patientId = Guid.NewGuid();
        var first = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var second = first.AddMinutes(6);

        var firstResult = service.Evaluate(patientId, Severity.Critical, first);
        var secondResult = service.Evaluate(patientId, Severity.Critical, second);

        Assert.False(firstResult.IsSuppressed);
        Assert.False(secondResult.IsSuppressed);
        Assert.Null(secondResult.RemainingCooldown);
    }

    [Fact]
    public void Evaluate_InfoSeverity_NeverSuppresses()
    {
        var service = new CooldownService();
        var patientId = Guid.NewGuid();

        var firstResult = service.Evaluate(patientId, Severity.Info, DateTime.UtcNow);
        var secondResult = service.Evaluate(patientId, Severity.Info, DateTime.UtcNow.AddSeconds(1));

        Assert.False(firstResult.IsSuppressed);
        Assert.False(secondResult.IsSuppressed);
        Assert.Null(firstResult.RemainingCooldown);
        Assert.Null(secondResult.RemainingCooldown);
    }
}