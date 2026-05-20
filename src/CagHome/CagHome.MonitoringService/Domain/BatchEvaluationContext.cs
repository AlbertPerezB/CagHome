using CagHome.Contracts;
using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Domain;

/// <summary>
/// Represents the inputs required to evaluate a received telemetry batch against a careplan.
/// </summary>
/// <param name="Batch">The incoming telemetry batch to evaluate.</param>
/// <param name="Careplan">The patient's active careplan used for policy evaluation.</param>
public record BatchEvaluationContext(
    BatchReceived Batch,
    Careplan Careplan
);
