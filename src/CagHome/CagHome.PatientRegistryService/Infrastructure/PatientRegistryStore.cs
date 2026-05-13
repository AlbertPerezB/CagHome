using CagHome.PatientRegistryService.Domain;
using MongoDB.Driver;

namespace CagHome.PatientRegistryService.Infrastructure;

internal class PatientRegistryStore : IPatientRegistryStore
{
    private readonly IMongoCollection<PatientRegistryEntry> _collection;

    public PatientRegistryStore(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("PatientRegistry");
        _collection = database.GetCollection<PatientRegistryEntry>("PatientData");
    }

    public async Task<UpdateResult> UpdatePatientData(PatientRegistryEntry entry)
    {
        var filter = Builders<PatientRegistryEntry>.Filter.And(
            Builders<PatientRegistryEntry>.Filter.Eq(e => e.PatientId, entry.PatientId),
            Builders<PatientRegistryEntry>.Filter.Lt(e => e.LastUpdatedUtc, entry.LastUpdatedUtc)
        );
        var update = Builders<PatientRegistryEntry>
            .Update.Set(e => e.Status, entry.Status)
            .Set(e => e.LastUpdatedUtc, entry.LastUpdatedUtc);

        var result = await _collection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = false }
        );

        // If no match (stale update), try upsert in case the patient doesn't exist yet
        if (result.MatchedCount == 0)
        {
            var upsertFilter = Builders<PatientRegistryEntry>.Filter.Eq(
                e => e.PatientId,
                entry.PatientId
            );
            result = await _collection.UpdateOneAsync(
                upsertFilter,
                update,
                new UpdateOptions { IsUpsert = true }
            );
        }

        return result;
    }

    public async Task<List<PatientRegistryEntry>> GetAllPatients()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }
}
