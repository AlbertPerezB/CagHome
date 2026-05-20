using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.BatchValidation;

/// <summary>
/// Validates a Batch against a set of rules, accumulating any validation errors
/// and identifying fatal errors that should stop further processing.
/// </summary>
public class BatchValidator
{
    private IEnumerable<IBatchValidationRule> Rules { get; }

    public BatchValidator(IEnumerable<IBatchValidationRule> rules)
    {
        Rules = rules;
    }

    public async Task<Batch> ValidateAsync(Batch input)
    {
        foreach (var rule in Rules)
        {
            var error = await rule.ValidateAsync(input);

            if (error != null)
            {
                input.ValidationErrors.Add(error);
                if (rule.IsFatal)
                {
                    input.FatalError = error;
                    break;
                }
            }
        }

        return input;
    }
}
