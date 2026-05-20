namespace CagHome.Contracts.Enums;

/// <summary>
/// Represents the severity level of a monitoring alert.
/// </summary>
public enum Severity
{
    Info,
    /// <summary>A condition that warrants attention but is not immediately critical.</summary>
    Warning,
    /// <summary>A serious condition requiring immediate clinical attention.</summary>
    Critical,
}
