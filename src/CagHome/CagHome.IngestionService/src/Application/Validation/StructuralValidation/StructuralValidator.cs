using System.Collections.Concurrent;
using System.Security.AccessControl;
using System.Text.Json;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.StructuralValidation;

public class StructuralValidator
{
    private IEnumerable<IValidationRule<JsonDocument>> Rules { get; }

    public StructuralValidator(IEnumerable<IValidationRule<JsonDocument>> rules)
    {
        Rules = rules;
    }

    public async Task<ValidationError?> ValidateAsync(JsonDocument input)
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
