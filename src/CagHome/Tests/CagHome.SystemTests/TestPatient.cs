using CagHome.Contracts.Enums;

namespace CagHome.SystemTests;

public class TestPatient
{
    public Guid PatientId;
    public Careplan Careplan { get; set; }
    public PatientStatus Status { get; set; }

    public static TestPatient ActiveCardiomyopathy() =>
        new()
        {
            PatientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Status = PatientStatus.Active,
            Careplan = Careplan.Cardiomyopathy,
        };

    public static TestPatient InactiveCoronaryArteryDisease() =>
        new()
        {
            PatientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Status = PatientStatus.Inactive,
            Careplan = Careplan.CoronaryArteryDisease,
        };

    public static TestPatient DeceasedValveDisease() =>
        new()
        {
            PatientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Status = PatientStatus.Deceased,
            Careplan = Careplan.ValveDisease,
        };

    public static List<TestPatient> All() =>
        new List<TestPatient>
        {
            ActiveCardiomyopathy(),
            InactiveCoronaryArteryDisease(),
            DeceasedValveDisease(),
        };
}
