using CagHome.Contracts.Enums;

namespace CagHome.MonitoringService.Infrastructure;

public interface IPatientCareplanStore
{
    Task Upsert(Guid patientId, Careplan careplan, DateTime updatedAtUtc);

    Task<Careplan?> TryGet(Guid patientId);
}
