using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation;

public interface IBatchValidationRule : IValidationRule<Batch>
{
    bool IsFatal { get; }
}
