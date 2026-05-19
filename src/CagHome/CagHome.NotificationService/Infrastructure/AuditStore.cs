using CagHome.NotificationService.Domain;
using MongoDB.Driver;

namespace CagHome.NotificationService.Infrastructure;

internal class AuditStore : IAuditStore
{
    private readonly IMongoCollection<AuditEntry> _collection;

    public AuditStore(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("notification-audit");
        _collection = database.GetCollection<AuditEntry>("NotificationAuditEntries");
    }

    public async Task RecordAuditEntry(AuditEntry entry)
    {
        await _collection.InsertOneAsync(entry);
    }
}
