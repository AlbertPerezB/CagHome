using MongoDB.Driver;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Infrastructure;

public sealed class MongoPatientCareplanStore : IPatientCareplanStore
{
    private readonly IMongoCollection<PatientCareplanState> _collection;

    public MongoPatientCareplanStore(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("MonitoringService");
        _collection = database.GetCollection<PatientCareplanState>("PatientCareplans");
    }

    public async Task Upsert(Guid patientId, Careplan careplan, DateTime updatedAtUtc)
    {
        var state = new PatientCareplanState
        {
            PatientId = patientId,
            Careplan = careplan,
            UpdatedAtUtc = updatedAtUtc,
        };

        await _collection.ReplaceOneAsync(
            filter: x => x.PatientId == patientId,
            replacement: state,
            options: new ReplaceOptions { IsUpsert = true }
        );
    }

    public async Task<Careplan?> TryGet(Guid patientId)
    {
        var state = await _collection.Find(x => x.PatientId == patientId).FirstOrDefaultAsync();
        return state?.Careplan;
    }
}
