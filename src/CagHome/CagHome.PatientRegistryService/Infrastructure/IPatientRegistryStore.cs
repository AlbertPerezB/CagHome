using CagHome.PatientRegistryService.Domain;
using MongoDB.Driver;

namespace CagHome.PatientRegistryService.Infrastructure
{
    /// <summary>
    /// Defines methods for updating and retrieving patient registry data.
    /// </summary>
    /// <remarks>Implementations of this interface provide access to a collection of patient registry entries,
    /// supporting both retrieval of all entries and updating individual patient data. Thread safety and persistence
    /// guarantees depend on the specific implementation.</remarks>
    public interface IPatientRegistryStore
    {
        /// <summary>
        /// Updates the patient data in the registry with the information provided in the specified <see cref="PatientRegistryEntry"/>.
        /// </summary>
        /// <param name="entry">The patient registry entry containing updated patient information. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous update operation. The task result contains an <see cref="UpdateResult"/>
        /// indicating the outcome of the update.</returns>
        Task<UpdateResult> UpdatePatientData(PatientRegistryEntry entry);

        /// <summary>
        /// Retrieves all patient entries from the registry.
        /// </summary>
        /// <returns>A task that represents the asynchronous retrieval operation. The task result contains a list of <see cref="PatientRegistryEntry"/>
        /// objects representing all patients in the registry.</returns>
        Task<List<PatientRegistryEntry>> GetAllPatients();
    }
}
