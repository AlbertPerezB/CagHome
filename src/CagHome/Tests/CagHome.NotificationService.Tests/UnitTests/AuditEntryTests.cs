using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.NotificationService.Domain;

namespace CagHome.NotificationService.Tests.Domain;

public class AuditEntryTests
{
    [Fact]
    public void Constructor_WithHospitalAlert_SetsReceiverToHospital()
    {
        var message = new HospitalAlertRequested(
            AlertId: Guid.NewGuid(),
            DecidedAt: DateTime.UtcNow,
            HospitalId: Guid.NewGuid(),
            Message: "High heart rate. Patient risks going into SVT",
            PatientId: Guid.NewGuid(),
            Severity: Severity.Critical
        );

        var entry = new AuditEntry(message, DeliveryStatus.Attempted, "200");

        Assert.Equal(Receiver.Hospital, entry.Receiver);
        Assert.Equal(message.AlertId, entry.AlertId);
        Assert.Equal(message.PatientId, entry.PatientId);
        Assert.Equal(message.HospitalId, entry.HospitalId);
        Assert.Equal(message.Message, entry.Message);
        Assert.Equal(DeliveryStatus.Attempted, entry.DeliveryStatus);
        Assert.Equal("200", entry.StatusCode);
    }

    [Fact]
    public void Constructor_WithoutStatusCode_SetsEmptyStatusCode()
    {
        var message = new HospitalAlertRequested(
            AlertId: Guid.NewGuid(),
            DecidedAt: DateTime.UtcNow,
            HospitalId: Guid.NewGuid(),
            Message: "High heart rate. Patient risks going into SVT",
            PatientId: Guid.NewGuid(),
            Severity: Severity.Critical
        );

        var entry = new AuditEntry(message, DeliveryStatus.Attempted);

        Assert.Equal(Receiver.Hospital, entry.Receiver);
        Assert.Equal(message.AlertId, entry.AlertId);
        Assert.Equal(message.PatientId, entry.PatientId);
        Assert.Equal(message.HospitalId, entry.HospitalId);
        Assert.Equal(message.Message, entry.Message);
        Assert.Equal(DeliveryStatus.Attempted, entry.DeliveryStatus);
        Assert.Empty(entry.StatusCode);
    }

    [Fact]
    public void Constructor_WithPatientAlert_SetsReceiverToPatientAndNullHospitalId()
    {
        var message = new PatientAlertRequested(
            AlertId: Guid.NewGuid(),
            DecidedAt: DateTime.UtcNow,
            Message: "High heart rate. Patient risks going into SVT",
            PatientId: Guid.NewGuid(),
            Severity: Severity.Critical
        );

        var entry = new AuditEntry(message, DeliveryStatus.Delivered);

        Assert.Equal(Receiver.Patient, entry.Receiver);
        Assert.Equal(message.AlertId, entry.AlertId);
        Assert.Equal(message.PatientId, entry.PatientId);
        Assert.Null(entry.HospitalId);
        Assert.Empty(entry.StatusCode);
        Assert.Equal(message.Message, entry.Message);
        Assert.Equal(DeliveryStatus.Delivered, entry.DeliveryStatus);
    }
}
