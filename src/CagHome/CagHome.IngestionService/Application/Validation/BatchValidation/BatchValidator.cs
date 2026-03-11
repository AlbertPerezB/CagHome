using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.BatchValidation;

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
