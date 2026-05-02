using CagHome.PatientRegistryService.Domain;
using MongoDB.Driver;

namespace CagHome.PatientRegistryService.Infrastructure
{
    public interface IPatientRegistryStore
    {
        Task<UpdateResult> UpdatePatientData(PatientRegistryEntry entry);
    }
}
