using CagHome.Contracts.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CagHome.PatientRegistryService.Domain
{
    public class PatientRegistryEntry
    {
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid PatientId;

        public PatientStatus Status;

        public DateTime LastUpdatedUtc;
    }
}
