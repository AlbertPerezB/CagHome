namespace CagHome.IngestionService.Application.Validation;

public interface IValidationRule<T>
{
    Task<ValidationResult> ValidateAsync(T input, CancellationToken ct = default);
}
