using System.Collections.Concurrent;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation;

public abstract class Validator<T>
{
    protected readonly List<IValidationRule<T>> Rules;

    protected Validator(List<IValidationRule<T>> rules)
    {
        Rules = rules;
    }

    protected async Task<(bool fatal, List<ValidationError> errors)> ValidateSequential(T target)
    {
        var errors = new List<ValidationError>();

        foreach (var rule in Rules)
        {
            var error = await rule.ValidateAsync(target);

            if (error == null)
                continue;

            errors.Add(error);

            if (rule.IsFatal)
                return (true, errors);
        }

        return (false, errors);
    }

    protected async Task<List<ValidationError>> ValidateParallel(IEnumerable<T> items)
    {
        var errors = new ConcurrentBag<ValidationError>();

        await Parallel.ForEachAsync(
            items,
            async (item, _) =>
            {
                foreach (var rule in Rules)
                {
                    var result = await rule.ValidateAsync(item);

                    if (result != null)
                        errors.Add(result);
                }
            }
        );

        return errors.ToList();
    }
}
