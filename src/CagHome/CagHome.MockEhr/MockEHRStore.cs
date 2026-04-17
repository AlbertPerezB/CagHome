using System.Collections.Concurrent;
using CagHome.Contracts;

namespace CagHome.MockEhr;

/// <summary>
/// Simple in-memory store for the mock EHR.
/// In a real EHR this would be a database — here it's just
/// thread-safe collections so the endpoints have something to read/write.
/// </summary>
public class MockEhrStore
{
    public ConcurrentBag<ReceivedAlert> Alerts { get; } = [];

    public ConcurrentQueue<ClinicianResponse> ClinicianResponses { get; } = new();

    public ConcurrentQueue<PatientRegistration> PatientRegistrations { get; } = new();
}

/// <summary>
/// An alert as received and stored by the mock EHR.
/// </summary>
public record ReceivedAlert(
    Guid AlertId,
    Guid PatientId,
    Guid HospitalId,
    string Message,
    Severity Severity,
    DateTime ReceivedAtUtc
);

/// <summary>
/// A clinician response waiting to be picked up by polling.
/// </summary>
public record ClinicianResponse(
    Guid ResponseId,
    Guid AlertId,
    Guid PatientId,
    string Message,
    DateTime CreatedAtUtc
);

/// <summary>
/// A new patient registration waiting to be picked up by polling.
/// </summary>
public record PatientRegistration(Guid PatientId, string Name, DateTime RegisteredAtUtc);
