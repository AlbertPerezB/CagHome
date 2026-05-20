using MongoDB.Driver;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Infrastructure;

/// <summary>
/// MongoDB implementation of <see cref="IDecisionAuditStore"/>.
/// </summary>
public class MongoDecisionAuditStore : IDecisionAuditStore
{
    private readonly IMongoCollection<DecisionAuditEntry> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDecisionAuditStore"/> class.
    /// </summary>
    /// <param name="mongoClient">MongoDB client used to access the monitoring audit database.</param>
    public MongoDecisionAuditStore([FromKeyedServices("monitoring-audit")] IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("monitoring-audit");
        _collection = database.GetCollection<DecisionAuditEntry>("DecisionAuditEntries");
    }

    /// <summary>
    /// Record a decision audit entry to MongoDB.
    /// </summary>
    /// <param name="entry">The decision audit entry to store.</param>
    /// <returns>A task when the entry is inserted.</returns>
    public async Task RecordAuditEntry(DecisionAuditEntry entry)
    {
        await _collection.InsertOneAsync(entry);
    }
}
