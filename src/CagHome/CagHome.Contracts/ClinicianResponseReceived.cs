namespace CagHome.Contracts;

/// <summary>
/// Represents a clinician's response to an alert.
/// </summary>
/// <param name="AlertId">The id of the alert being responded to.</param>
/// <param name="CreatedAtUtc">The UTC timestamp when the response was created.</param>
/// <param name="CorrelationId">The id used to correlate related events across services.</param>
/// <param name="HospitalId">The id of the hospital the clinician belongs to.</param>
/// <param name="Message">The clinician's response message.</param>
/// <param name="PatientId">The unique id of the patient the alert concerns.</param>
/// <param name="ResponseId">The unique id of this response.</param>
public record ClinicianResponseReceived(
    Guid AlertId,
    DateTime CreatedAtUtc,
    Guid CorrelationId,
    Guid HospitalId,
    string Message,
    Guid PatientId,
    Guid ResponseId
);
