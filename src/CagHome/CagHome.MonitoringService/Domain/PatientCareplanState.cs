using CagHome.Contracts.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CagHome.MonitoringService.Domain;

/// <summary>
/// Represents the careplan state for a patient.
/// </summary>
public class PatientCareplanState
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid PatientId { get; init; }
    public Careplan Careplan { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}