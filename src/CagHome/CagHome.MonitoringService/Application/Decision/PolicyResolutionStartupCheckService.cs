using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;

namespace CagHome.MonitoringService.Application.Decision;

/// <summary>
/// Validates careplan-to-policy resolution during service startup.
/// </summary>
/// <param name="policyResolver">Resolver used to map each careplan to a policy.</param>
/// <param name="logger">Logger for validation outcomes.</param>
public class PolicyResolutionStartupCheckService(
    ICareplanPolicyResolver policyResolver,
    ILogger<PolicyResolutionStartupCheckService> logger
) : IHostedService
{
    /// <summary>
    /// Validates that every careplan can be resolved to a policy at startup.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel startup operations.</param>
    /// <returns>A completed task when validation succeeds.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var careplan in Enum.GetValues<Careplan>())
        {
            try
            {
                var policy = policyResolver.Resolve(careplan);
                logger.LogDebug(
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

    /// <summary>
    /// Stops the startup check service.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel stop operations.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
