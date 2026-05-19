using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.BatchValidation;

public interface IBatchValidationRule : IValidationRule<Batch>
{
    bool IsFatal { get; }
}
