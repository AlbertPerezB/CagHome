using CagHome.PatientRegistryService.Domain;
using MongoDB.Driver;

namespace CagHome.PatientRegistryService.Infrastructure;

/// <summary>
/// The store responsible for managing patient registry data, providing methods to update patient information
/// and retrieve all patient entries. This class interacts with a MongoDB collection to persist patient data
/// and ensures that updates are applied only if they are more recent than existing records. It also supports
/// upserting new patient entries when necessary.
/// </summary>
internal class PatientRegistryStore : IPatientRegistryStore
{
    private readonly IMongoCollection<PatientRegistryEntry> _collection;

    public PatientRegistryStore(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("patient-registry");
        _collection = database.GetCollection<PatientRegistryEntry>("PatientData");
    }

    /// <summary>
    /// Updates the patient data in the registry with the information provided in the specified <see cref="PatientRegistryEntry"/>.
    /// </summary>
    /// <param name="entry">The <see cref="PatientRegistryEntry"/> containing updated patient information. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous update operation. The task result contains an <see cref="UpdateResult"/>
    /// indicating the outcome of the update.</returns>
    public async Task<UpdateResult> UpdatePatientData(PatientRegistryEntry entry)
    {
        var filter = Builders<PatientRegistryEntry>.Filter.And(
            Builders<PatientRegistryEntry>.Filter.Eq(e => e.PatientId, entry.PatientId),
            Builders<PatientRegistryEntry>.Filter.Lt(e => e.LastUpdatedUtc, entry.LastUpdatedUtc),
            Builders<PatientRegistryEntry>.Filter.Ne(e => e.Status, entry.Status)
        );

        var update = Builders<PatientRegistryEntry>
            .Update.Set(e => e.Status, entry.Status)
            .Set(e => e.LastUpdatedUtc, entry.LastUpdatedUtc);

        var result = await _collection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = false }
        );

        // No match: either new patient or no change
        if (result.MatchedCount == 0)
        {
            var exists = await _collection
                .Find(Builders<PatientRegistryEntry>.Filter.Eq(e => e.PatientId, entry.PatientId))
                .AnyAsync();

            if (!exists)
            {
                await _collection.InsertOneAsync(entry);
                return new UpdateResult.Acknowledged(
                    matchedCount: 0,
                    modifiedCount: 0,
                    upsertedId: new MongoDB.Bson.BsonString(entry.PatientId.ToString())
                );
            }
        }

        return result;
    }

    /// <summary>
    /// Retrieves all patient entries from the MongoDB collection and returns them as a list.
    /// </summary>
    /// <returns>A task that represents the asynchronous retrieval operation. The task result contains a list of <see cref="PatientRegistryEntry"/>
    /// objects representing the patients in the registry.</returns>
    public async Task<List<PatientRegistryEntry>> GetAllPatients()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }
}
