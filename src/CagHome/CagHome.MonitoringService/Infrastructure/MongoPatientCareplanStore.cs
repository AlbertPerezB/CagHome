using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Domain;
using MongoDB.Driver;

namespace CagHome.MonitoringService.Infrastructure;

public class MongoPatientCareplanStore : IPatientCareplanStore
{
    private readonly IMongoCollection<PatientCareplanState> _collection;

    public MongoPatientCareplanStore(
        [FromKeyedServices("monitoring-patient-careplans")] IMongoClient mongoClient
    )
    {
        var database = mongoClient.GetDatabase("monitoring-patient-careplans");
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
