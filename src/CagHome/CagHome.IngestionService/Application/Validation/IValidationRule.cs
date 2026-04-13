using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation;

public interface IValidationRule<T>
{
    Task<ValidationError?> ValidateAsync(T input);
}
