using System.Text.Json;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.StructuralValidation;

/// <summary>
/// Validator that applies a set of structural validation rules to a JSON document.
/// Each rule checks for specific structural requirements (e.g. presence of required fields, correct data types)
/// and returns the first ValidationError encountered, if any, otherwise null.
/// </summary>
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
