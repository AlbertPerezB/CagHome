using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Application.Decision.Interfaces;

/// <summary>
/// Defines a careplan-specific policy used to evaluate incoming telemetry batches.
/// </summary>
public interface ICareplanDecisionPolicy
{
    /// <summary>
    /// Gets the careplan this policy uses for evaluating.
    /// </summary>
    Careplan Careplan { get; }

    /// <summary>
    /// Evaluates a telemetry batch in the context of the associated careplan.
    /// </summary>
    /// <param name="context">The evaluation context containing batch data and careplan state.</param>
    /// <returns>The policy decision result for the provided context.</returns>
    PolicyDecisionResult Evaluate(BatchEvaluationContext context);
}
