using System;
using System.Collections.Generic;
using System.Text;
using CagHome.Contracts.Enums;

namespace CagHome.Contracts
{
    public record AllPatientStatusesRequested();

    public record AllPatientStatuses(List<PatientStatusEntry> Patients);

    public record PatientStatusEntry(Guid PatientId, PatientStatus Status);
}
