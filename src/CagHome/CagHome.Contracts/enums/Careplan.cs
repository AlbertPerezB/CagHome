namespace CagHome.Contracts.Enums;

/// <summary>
/// Identifies the careplan assigned to a patient, determining which monitoring policy applies.
/// </summary>
public enum Careplan
{
    /// <summary>No specific careplan assigned; uses default monitoring thresholds.</summary>
    None,
    /// <summary>Careplan for patients with valve disease.</summary>
    ValveDisease,
    /// <summary>Careplan for patients with coronary artery disease.</summary>
    CoronaryArteryDisease,
    /// <summary>Careplan for patients with cardiomyopathy.</summary>
    Cardiomyopathy,
}
