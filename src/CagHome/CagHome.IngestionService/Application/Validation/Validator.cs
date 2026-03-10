using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation;

public abstract class Validator<T> : IValidator<T>
{
    protected abstract IEnumerable<IValidationRule<T>> Rules { get; }

    public async Task<ValidationOutcome> ValidateAsync(T item, CancellationToken ct)
    {
        var results = new Dictionary<string, ValidationResult>();

        await Task.WhenAll(
            Rules.Select(async rule =>
            {
                var result = await rule.ValidateAsync(item, ct);
                if (!result.IsValid)
                {
                    results[rule.GetType().Name] = result;
                    if (rule.StopOnFailure)
                    {
                        return;
                    }
                }

                results[rule.GetType().Name] = result;
            })
        );

        return new ValidationOutcome { Results = results };
    }
}
