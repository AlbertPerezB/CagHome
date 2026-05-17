using System.Collections.Concurrent;
using CagHome.Contracts.Enums;
using CagHome.MonitoringService.Infrastructure;

namespace CagHome.MonitoringService.Tests.Helpers;

public sealed class PatientCareplanStore : IPatientCareplanStore
{
    private readonly ConcurrentDictionary<Guid, (Careplan Careplan, DateTime UpdatedAtUtc)> _careplans =
        new();

    public Task Upsert(Guid patientId, Careplan careplan, DateTime updatedAtUtc)
    {
        _careplans[patientId] = (careplan, updatedAtUtc);
        return Task.CompletedTask;
    }

    public Task<Careplan?> TryGet(Guid patientId)
    {
        var exists = _careplans.TryGetValue(patientId, out var value);
        return Task.FromResult(exists ? value.Careplan : (Careplan?)null);
    }

    public void Clear()
    {
        _careplans.Clear();
    }
}
