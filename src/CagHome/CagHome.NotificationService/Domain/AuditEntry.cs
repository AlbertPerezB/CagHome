using System.Diagnostics;
using CagHome.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CagHome.NotificationService.Domain;

public class AuditEntry
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid AlertId { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid? HospitalId { get; set; }
    public string? Message { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid PatientId { get; set; }
    public Receiver Receiver { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; } = string.Empty;

    public AuditEntry(HospitalAlertRequested message, DeliveryStatus status, string statusCode = "")
    {
        AlertId = message.AlertId;
        DeliveryStatus = status;
        HospitalId = message.HospitalId;
        Message = message.Message;
        PatientId = message.PatientId;
        Receiver = Receiver.Hospital;
        StatusCode = statusCode;
        Timestamp = DateTime.UtcNow;
        TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
    }

    public AuditEntry(PatientAlertRequested message, DeliveryStatus status)
    {
        AlertId = message.AlertId;
        DeliveryStatus = status;
        HospitalId = null;
        Message = message.Message;
        PatientId = message.PatientId;
        Receiver = Receiver.Patient;
        StatusCode = string.Empty;
        Timestamp = DateTime.UtcNow;
        TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
    }

    public AuditEntry(ClinicianResponseReceived message, DeliveryStatus status)
    {
        AlertId = message.AlertId;
        DeliveryStatus = status;
        HospitalId = message.HospitalId;
        Message = message.Message;
        PatientId = message.PatientId;
        Receiver = Receiver.Patient;
        StatusCode = string.Empty;
        Timestamp = DateTime.UtcNow;
        TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
    }
}
