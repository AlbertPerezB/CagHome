using CagHome.Contracts.Enums;

namespace CagHome.PatientRegistryService.Domain
{
    public record PatientRegistryEntry(
        Guid PatientId,
        PatientStatus Status,
        DateTime LastUpdatedUtc
    );
}
