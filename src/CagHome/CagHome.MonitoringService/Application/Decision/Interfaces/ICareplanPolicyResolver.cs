using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Application.Decision.Interfaces;

/// <summary>
/// Resolves the policy implementation for a given careplan.
/// </summary>
public interface ICareplanPolicyResolver
{
    /// <summary>
    /// Gets the decision policy corresponding to the specified careplan.
    /// </summary>
    /// <param name="careplan">The careplan for which to resolve a policy.</param>
    /// <returns>The policy implementation that evaluates the provided careplan.</returns>
    ICareplanDecisionPolicy Resolve(Careplan careplan);
}
