using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Application.Decision.Interfaces;

namespace CagHome.MonitoringService.Application.Decision;

public sealed class CareplanPolicyResolver : ICareplanPolicyResolver
{
    private readonly IReadOnlyDictionary<Careplan, ICareplanDecisionPolicy> _policies;

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