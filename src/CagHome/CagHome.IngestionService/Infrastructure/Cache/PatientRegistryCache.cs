using CagHome.Contracts.Enums;
using StackExchange.Redis;

namespace CagHome.IngestionService.Infrastructure.Cache;

public class PatientRegistryCache : IPatientRegistryCache
{
    private readonly IDatabase _db;

    public PatientRegistryCache(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task SetPatientStatus(Guid patientId, PatientStatus status)
    {
        await _db.StringSetAsync($"patient:{patientId}:status", status.ToString());
    }

    public async Task<PatientStatus?> GetPatientStatus(Guid patientId)
    {
        var value = await _db.StringGetAsync($"patient:{patientId}:status");
        if (value.IsNullOrEmpty)
            return null;
        return Enum.Parse<PatientStatus>(value!);
    }
}
