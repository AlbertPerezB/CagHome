using System.Diagnostics;
using CagHome.NotificationService.Domain;
using MongoDB.Driver;

namespace CagHome.NotificationService.Infrastructure;

internal class AuditStore
{
    private readonly IMongoCollection<AuditEntry> _collection;

    public AuditStore(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("NotificationService");
        _collection = database.GetCollection<AuditEntry>("AuditEntries");
    }

    public async Task RecordAuditEntry(AuditEntry entry)
    {
        await _collection.InsertOneAsync(entry);
    }
}
