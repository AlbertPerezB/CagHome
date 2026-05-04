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
        var filter = Builders<PatientRegistryEntry>.Filter.Eq(e => e.PatientId, entry.PatientId);
        var update = Builders<PatientRegistryEntry>
            .Update.Set(e => e.Status, entry.Status)
            .Set(e => e.LastUpdatedUtc, entry.LastUpdatedUtc);

        var result = await _collection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true }
        );
        return result;
    }
}
