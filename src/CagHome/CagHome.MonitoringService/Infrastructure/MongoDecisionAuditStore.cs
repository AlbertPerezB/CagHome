using MongoDB.Driver;
using CagHome.MonitoringService.Domain;

namespace CagHome.MonitoringService.Infrastructure;

public sealed class MongoDecisionAuditStore : IDecisionAuditStore
{
    private readonly IMongoCollection<DecisionAuditEntry> _collection;

    public MongoDecisionAuditStore(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("MonitoringService");
        _collection = database.GetCollection<DecisionAuditEntry>("DecisionAuditEntries");
    }

    public async Task RecordAuditEntry(DecisionAuditEntry entry)
    {
        await _collection.InsertOneAsync(entry);
    }
}
