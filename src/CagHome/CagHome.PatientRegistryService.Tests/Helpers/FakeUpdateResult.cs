using MongoDB.Bson;
using MongoDB.Driver;

namespace CagHome.PatientRegistryService.Tests.Helpers;

public static class FakeUpdateResult
{
    /// <summary>
    /// Simulates an existing document being modified.
    /// </summary>
    public static UpdateResult Modified(long modifiedCount = 1) =>
        new UpdateResult.Acknowledged(
            matchedCount: modifiedCount,
            modifiedCount: modifiedCount,
            upsertedId: null
        );

    /// <summary>
    /// Simulates a new document being inserted via upsert.
    /// </summary>
    public static UpdateResult Upserted() =>
        new UpdateResult.Acknowledged(
            matchedCount: 0,
            modifiedCount: 0,
            upsertedId: new BsonObjectId(ObjectId.GenerateNewId())
        );

    /// <summary>
    /// Simulates a write where nothing changed.
    /// </summary>
    public static UpdateResult NoChange() =>
        new UpdateResult.Acknowledged(matchedCount: 1, modifiedCount: 0, upsertedId: null);

    /// <summary>
    /// Simulates an unacknowledged write.
    /// </summary>
    public static UpdateResult Unacknowledged() => UpdateResult.Unacknowledged.Instance;
}
