using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.MeasurementValidation;

/// <summary>
/// Validates a Measurement against a set of rules, returning any validation errors found.
/// Each rule is applied independently, allowing for multiple errors to be collected in a single pass.
/// </summary>
public class MeasurementValidator
{
    private IEnumerable<IValidationRule<Measurement>> Rules { get; }

    public MeasurementValidator(IEnumerable<IValidationRule<Measurement>> rules)
    {
        Rules = rules;
    }

    public Task<Measurement> ValidateAsync(Measurement input)
    {
        Task.WhenAll(
            Rules.Select(async rule =>
            {
                var error = await rule.ValidateAsync(input);

                if (error != null)
                    input.ValidationErrors.Add(error);
            })
        );
        return Task.FromResult(input);
    }
}
