using System.Collections.Concurrent;
using System.Security.AccessControl;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.StructuralValidation;

public class StructuralValidator
{
    private IEnumerable<IValidationRule<RawBatch>> Rules { get; }

    public StructuralValidator(IEnumerable<IValidationRule<RawBatch>> rules)
    {
        Rules = rules;
    }

    public async Task<ValidationError?> ValidateAsync(RawBatch input)
    {
        foreach (var rule in Rules)
        {
            var error = await rule.ValidateAsync(input);

            if (error != null)
            {
                return error;
            }
        }

        return null;
    }
}
