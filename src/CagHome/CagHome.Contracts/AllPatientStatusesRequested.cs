using CagHome.Contracts.Enums;

namespace CagHome.Contracts
{
    /// <summary>
    /// Message requesting the current status of all patients.
    /// </summary>
    public record AllPatientStatusesRequested();

    /// <summary>
    /// Contains the current status for all patients.
    /// </summary>
    /// <param name="Patients">The list of patient status entries.</param>
    public record AllPatientStatuses(List<PatientStatusEntry> Patients);

    /// <summary>
    /// Represents the status of a single patient.
    /// </summary>
    /// <param name="PatientId">The unique id of the patient.</param>
    /// <param name="Status">The current status of the patient.</param>
    public record PatientStatusEntry(Guid PatientId, PatientStatus Status);
}
