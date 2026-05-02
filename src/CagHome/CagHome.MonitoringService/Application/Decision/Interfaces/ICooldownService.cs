using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Application.Decision.Interfaces;

public interface ICooldownService
{
    CooldownCheckResult Evaluate(Guid patientId, Severity severity, DateTime timestampUtc);
}
