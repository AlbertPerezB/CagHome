using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Domain;
using MongoDB.Driver;

namespace CagHome.MonitoringService.Infrastructure;

/// <summary>
/// MongoDB implementation of <see cref="IPatientCareplanStore"/>.
/// </summary>
public class MongoPatientCareplanStore : IPatientCareplanStore
{
    private readonly IMongoCollection<PatientCareplanState> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoPatientCareplanStore"/> class.
    /// </summary>
    /// <param name="mongoClient">MongoDB client used to access the patient careplan database.</param>
    public MongoPatientCareplanStore(
        [FromKeyedServices("monitoring-patient-careplans")] IMongoClient mongoClient
    )
    {
        var database = mongoClient.GetDatabase("monitoring-patient-careplans");
        _collection = database.GetCollection<PatientCareplanState>("PatientCareplans");
    }

    /// <summary>
    /// Inserts or updates the careplan state for a patient.
    /// </summary>
    /// <param name="patientId">The unique id of the patient.</param>
    /// <param name="careplan">The careplan to persist.</param>
    /// <param name="updatedAtUtc">The UTC timestamp representing when this careplan was updated.</param>
    /// <returns>A task when the upsert operation is finished.</returns>
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

    /// <summary>
    /// Attempts to retrieve the latest careplan for a patient.
    /// </summary>
    /// <param name="patientId">The unique id of the patient.</param>
    /// <returns>The careplan when found, otherwise, <see langword="null"/>.</returns>
    public async Task<Careplan?> TryGet(Guid patientId)
    {
        var state = await _collection.Find(x => x.PatientId == patientId).FirstOrDefaultAsync();
        return state?.Careplan;
    }
}
