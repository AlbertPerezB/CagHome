using System.Collections.Concurrent;
using CagHome.MockEhr.Domain;

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

    public ConcurrentQueue<PatientRegistrationUpdate> PatientRegistrations { get; } = new();
}
