using System.Diagnostics;
using CagHome.Contracts.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CagHome.MonitoringService.Domain;

public sealed class DecisionAuditEntry
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid AuditId { get; init; } = Guid.NewGuid();

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid PatientId { get; init; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid BatchId { get; init; }
    public Careplan Careplan { get; init; }
    public string PolicyName { get; init; } = string.Empty;
    public Severity? Severity { get; init; }
    public bool ShouldAlertPatient { get; init; }
    public bool ShouldAlertHospital { get; init; }
    public bool SuppressedByCooldown { get; init; }
    public TimeSpan? RemainingCooldown { get; init; }
    public bool PatientAlertPublished { get; init; }
    public bool HospitalAlertPublished { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<DecisionReason> Reasons { get; init; } = [];
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    public string TraceId { get; init; } = Activity.Current?.TraceId.ToString() ?? string.Empty;
}
