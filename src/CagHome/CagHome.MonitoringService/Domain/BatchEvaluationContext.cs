using CagHome.Contracts;
using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Domain;

public sealed record BatchEvaluationContext(
    BatchReceived Batch,
    Careplan Careplan
);
