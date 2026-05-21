using CagHome.Contracts.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CagHome.PatientRegistryService.Domain
{
    /// <summary>
    /// Represents an entry in the patient registry, containing the patient's unique identifier, current status, and the
    /// timestamp of the last update.
    /// </summary>
    public class PatientRegistryEntry
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid PatientId;

        public PatientStatus Status;

        public DateTime LastUpdatedUtc;
    }
}
