using CagHome.Contracts;
using CagHome.Contracts.Enums;

namespace CagHome.NotificationService.Tests.Helpers
{
    public static class TestDataFactory
    {
        public static ClinicianResponseReceived CreateClinicianResponseReceived() =>
            new ClinicianResponseReceived(
                AlertId: Guid.NewGuid(),
                CreatedAtUtc: DateTime.UtcNow,
                CorrelationId: Guid.NewGuid(),
                HospitalId: Guid.NewGuid(),
                Message: "Lay down and keep feet high. An ambulance is on the way.  ",
                PatientId: Guid.NewGuid(),
                ResponseId: Guid.NewGuid()
            );

        public static HospitalAlertRequested CreateHospitalAlertRequested() =>
            new HospitalAlertRequested(
                AlertId: Guid.NewGuid(),
                CorrelationId: Guid.NewGuid(),
                DecidedAt: DateTime.UtcNow,
                HospitalId: Guid.NewGuid(),
                Message: "High heart rate. Patient risks going into SVT",
                PatientId: Guid.NewGuid(),
                Severity: Severity.Critical
            );

        public static PatientAlertRequested CreatePatientAlertRequested() =>
            new PatientAlertRequested(
                AlertId: Guid.NewGuid(),
                CorrelationId: Guid.NewGuid(),
                DecidedAt: DateTime.UtcNow,
                Message: "Your heart rate is high. Lay down.",
                PatientId: Guid.NewGuid(),
                Severity: Severity.Critical
            );
    }
}
