using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Application.Decision.Interfaces;

public interface ICareplanDecisionPolicy
{
    Careplan Careplan { get; }

    PolicyDecisionResult Evaluate(BatchEvaluationContext context);
}
