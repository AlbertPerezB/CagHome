using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation;

public interface IValidationRule<T>
{
    bool IsFatal { get; }

    Task<ValidationError?> ValidateAsync(T input);
}
