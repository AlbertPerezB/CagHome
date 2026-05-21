using System.Diagnostics;
using CagHome.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CagHome.NotificationService.Domain;

/// <summary>
/// Represents an audit record for alert delivery and response events, capturing relevant identifiers, status, and
/// metadata for tracking and analysis.
/// </summary>
/// <remarks>An AuditEntry contains information about the delivery or response of alerts to hospitals or patients,
/// including correlation and trace identifiers for distributed tracing. </remarks>
public class AuditEntry
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid AlertId { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid? HospitalId { get; set; } = null;
    public string? Message { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid CorrelationId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid PatientId { get; set; }
    public Receiver Receiver { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid? ResponseId { get; set; } = null;
    public string StatusCode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the AuditEntry class using a <see cref="HospitalAlertRequested"/> message, delivery status,
    /// and optional status code.
    /// </summary>
    /// <param name="message">The hospital alert request message containing alert details. Cannot be null.</param>
    /// <param name="status">The delivery status to associate with this audit entry.</param>
    /// <param name="statusCode">An optional status code that provides additional information about the delivery status. The default is an empty
    /// string.</param>
    public AuditEntry(HospitalAlertRequested message, DeliveryStatus status, string statusCode = "")
    {
        AlertId = message.AlertId;
        DeliveryStatus = status;
        HospitalId = message.HospitalId;
        CorrelationId = message.CorrelationId;
        Message = message.Message;
        PatientId = message.PatientId;
        Receiver = Receiver.Hospital;
        StatusCode = statusCode;
        Timestamp = DateTime.UtcNow;
        TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the AuditEntry class using the <see cref="PatientAlertRequested"/> message request and delivery
    /// status.
    /// </summary>
    /// <param name="message">The patient alert request message containing alert details to be audited. Cannot be null.</param>
    /// <param name="status">The delivery status to associate with this audit entry.</param>
    public AuditEntry(PatientAlertRequested message, DeliveryStatus status)
    {
        AlertId = message.AlertId;
        DeliveryStatus = status;
        CorrelationId = message.CorrelationId;
        Message = message.Message;
        PatientId = message.PatientId;
        Receiver = Receiver.Patient;
        StatusCode = string.Empty;
        Timestamp = DateTime.UtcNow;
        TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the AuditEntry class using the <see cref="ClinicianResponseReceived"/> message request and delivery
    /// status.
    /// </summary>
    /// <param name="message">The clinician response received message containing the message detials to be audited. Cannot be null.</param>
    /// <param name="status">The delivery status to associate with this audit entry.</param>
    public AuditEntry(ClinicianResponseReceived message, DeliveryStatus status)
    {
        AlertId = message.AlertId;
        DeliveryStatus = status;
        HospitalId = message.HospitalId;
        CorrelationId = message.CorrelationId;
        Message = message.Message;
        PatientId = message.PatientId;
        Receiver = Receiver.Patient;
        ResponseId = message.ResponseId;
        StatusCode = string.Empty;
        Timestamp = DateTime.UtcNow;
        TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
    }
}
