using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation;

/// <summary>
/// Defines a validation rule for a given type T. Implementations of this interface should provide logic
/// to validate an instance of T and return a ValidationError if the validation fails, or null if it succeeds.
/// </summary>
public interface IValidationRule<T>
{
    Task<ValidationError?> ValidateAsync(T input);
}
