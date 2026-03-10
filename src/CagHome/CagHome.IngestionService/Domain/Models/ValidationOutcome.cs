namespace CagHome.IngestionService.Domain.Models;

public record ValidationOutcome
{
    public bool IsValid => Results.Values.All(r => r.IsValid);
    public Dictionary<string, ValidationResult> Results { get; init; } = new();
}
