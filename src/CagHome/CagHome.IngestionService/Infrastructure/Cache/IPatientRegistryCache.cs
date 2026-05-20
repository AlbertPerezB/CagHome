using CagHome.Contracts.Enums;

namespace CagHome.IngestionService.Infrastructure.Cache;

/// <summary>
/// Local cache of patient statuses used during ingestion validation.
/// </summary>
public interface IPatientRegistryCache
{
    Task SetPatientStatus(Guid patientId, PatientStatus status);
    Task<PatientStatus?> GetPatientStatus(Guid patientId);
}
