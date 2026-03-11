using System.Collections.Concurrent;
using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.MeasurementValidation;

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
                    input.validationErrors.Add(error);
            })
        );
        return Task.FromResult(input);
    }
}
