using CagHome.Contracts;
using CagHome.Contracts.Enums;

namespace CagHome.NotificationService.Tests.Helpers;

/// <summary>
/// A test data factory for creating instances of message types used in the Notification Service.
/// This allows tests to easily generate valid test data without having to manually construct each message,
/// and ensures consistency across tests. Each method creates a new instance of the specified message type with
/// default values for its properties.
/// </summary>
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
