using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Application.Decision.Interfaces;

public interface ICareplanPolicyResolver
{
    ICareplanDecisionPolicy Resolve(Careplan careplan);
}
