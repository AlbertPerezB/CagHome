using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.BatchValidation;

/// <summary>
/// A validation rule relevant on batch level.
/// </summary>
public interface IBatchValidationRule : IValidationRule<Batch>
{
    /// <summary>
    /// Indicates whether the rule is fatal, meaning that a violation should stop further processing.
    /// </summary>
    bool IsFatal { get; }
}
