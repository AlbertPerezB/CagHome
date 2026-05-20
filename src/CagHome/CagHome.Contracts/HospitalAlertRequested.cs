using CagHome.Contracts.Enums;

namespace CagHome.Contracts;

/// <summary>
/// Message requesting that a hospital alert be raised for a patient.
/// </summary>
/// <param name="AlertId">The unique id of the alert.</param>
/// <param name="CorrelationId">The id used to correlate related events across services.</param>
/// <param name="DecidedAt">The UTC timestamp when the monitoring decision was made.</param>
/// <param name="HospitalId">The id of the hospital to receive the alert.</param>
/// <param name="Message">The alert message describing the detected condition.</param>
/// <param name="PatientId">The unique id of the patient triggering the alert.</param>
/// <param name="Severity">The severity level of the alert.</param>
public record HospitalAlertRequested(
    Guid AlertId,
    Guid CorrelationId,
    DateTime DecidedAt,
    Guid HospitalId,
    string Message,
    Guid PatientId,
    Severity Severity
);
