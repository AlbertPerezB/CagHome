namespace CagHome.IngestionService.Domain.Models;

public interface IValidationRule<T>
{
    bool StopOnFailure { get; }
    Task<ValidationResult> ValidateAsync(T input, CancellationToken ct = default);
}
