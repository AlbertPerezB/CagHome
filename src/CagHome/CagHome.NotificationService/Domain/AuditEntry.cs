using System.Diagnostics;
using CagHome.Contracts;
using OpenTelemetry.Trace;
using Wolverine.Persistence.Durability;

namespace CagHome.NotificationService.Domain;

public class AuditEntry
{
    public Guid AlertId { get; set; }
    public Guid? HospitalId { get; set; }
    public string? Message { get; set; }
    public Guid PatientId { get; set; }
    public Receiver Receiver { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; } = string.Empty;

    public AuditEntry(HospitalAlertRequested message, DeliveryStatus status)
    {
        AlertId = message.AlertId;
        DeliveryStatus = status;
        HospitalId = message.HospitalId;
        Message = message.Message;
        PatientId = message.PatientId;
        Receiver = Receiver.Hospital;
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
        Timestamp = DateTime.UtcNow;
        TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
    }
}
