using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;

namespace CagHome.MonitoringService.Application.Decision;

/// <summary>
/// Resolves careplan decision policy implementations.
/// </summary>
public class CareplanPolicyResolver : ICareplanPolicyResolver
{
    private readonly IReadOnlyDictionary<Careplan, ICareplanDecisionPolicy> _policies;

    /// <summary>
    /// Initializes a new instance of the <see cref="CareplanPolicyResolver"/> class.
    /// </summary>
    /// <param name="policies">The registered policy implementations keyed by careplan.</param>
    /// <exception cref="InvalidOperationException">Thrown when multiple policies are registered for the same careplan.</exception>
    public CareplanPolicyResolver(IEnumerable<ICareplanDecisionPolicy> policies)
    {
        var duplicateCareplans = policies
            .GroupBy(policy => policy.Careplan)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateCareplans.Length > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate careplan policies registered: {string.Join(", ", duplicateCareplans)}"
            );
        }

        _policies = policies.ToDictionary(policy => policy.Careplan);
    }

    /// <summary>
    /// Resolves a decision policy for the specified careplan.
    /// </summary>
    /// <param name="careplan">The careplan requiring a policy.</param>
    /// <returns>The matching policy, or the <see cref="Careplan.None"/> fallback policy when available.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no policy is registered for the requested careplan and no fallback policy exists.
    /// </exception>
    public ICareplanDecisionPolicy Resolve(Careplan careplan)
    {
        if (_policies.TryGetValue(careplan, out var policy))
        {
            return policy;
        }

        if (_policies.TryGetValue(Careplan.None, out var fallbackPolicy))
        {
            return fallbackPolicy;
        }

        throw new KeyNotFoundException(
            $"No policy registered for careplan '{careplan}', and no fallback policy for '{Careplan.None}'."
        );
    }
}