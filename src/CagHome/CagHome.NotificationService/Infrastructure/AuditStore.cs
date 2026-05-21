using CagHome.NotificationService.Domain;
using MongoDB.Driver;

namespace CagHome.NotificationService.Infrastructure;

/// <summary>
/// Provides functionality for recording audit entries to a MongoDB-backed audit store.
/// </summary>
/// <remarks>This class is intended for internal use and manages the persistence of audit data related to
/// notifications. It implements the IAuditStore interface to support audit logging within the application.</remarks>
internal class AuditStore : IAuditStore
{
    /// <summary>
    /// Represents the MongoDB collection used to store and retrieve audit entries.
    /// </summary>
    private readonly IMongoCollection<AuditEntry> _collection;

    /// <summary>
    /// Initializes a new instance of the AuditStore class using the specified MongoDB client.
    /// </summary>
    /// <param name="mongoClient">The MongoDB client used to connect to the audit database. Cannot be null.</param>
    public AuditStore(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("notification-audit");
        _collection = database.GetCollection<AuditEntry>("NotificationAuditEntries");
    }

    /// <summary>
    /// Asynchronously records an audit entry in the audit log.
    /// </summary>
    /// <param name="entry">The audit entry to record. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RecordAuditEntry(AuditEntry entry)
    {
        await _collection.InsertOneAsync(entry);
    }
}
