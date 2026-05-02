using CagHome.Contracts.Enums;
using StackExchange.Redis;

namespace CagHome.IngestionService.Infrastructure.Cache;

public interface IPatientRegistryCache
{
    Task SetPatientStatus(Guid patientId, PatientStatus status);
    Task<PatientStatus?> GetPatientStatus(Guid patientId);
}
