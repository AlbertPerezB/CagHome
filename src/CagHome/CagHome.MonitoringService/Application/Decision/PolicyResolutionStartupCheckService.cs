using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using Microsoft.Extensions.Logging;

namespace CagHome.MonitoringService.Application.Decision;

public sealed class PolicyResolutionStartupCheckService(
    ICareplanPolicyResolver policyResolver,
    ILogger<PolicyResolutionStartupCheckService> logger
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var careplan in Enum.GetValues<Careplan>())
        {
            try
            {
                var policy = policyResolver.Resolve(careplan);
                logger.LogInformation(
                    "Careplan policy mapping validated: Careplan={Careplan}, Policy={Policy}",
                    careplan,
                    policy.GetType().Name
                );
            }
            catch (Exception ex)
            {
                logger.LogCritical(
                    ex,
                    "Careplan policy mapping failed during startup validation for Careplan={Careplan}",
                    careplan
                );
                throw;
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
