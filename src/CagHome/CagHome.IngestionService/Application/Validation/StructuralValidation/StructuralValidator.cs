using System.Collections.Concurrent;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation;

public class StructuralValidator : Validator<RawBatch>
{
    public StructuralValidator(
        SchemaVersionFoundRule schemaVersionFoundRule,
        SchemaVersionSupportedRule schemaVersionSupportedRule
    )
        : base(
            new List<IValidationRule<RawBatch>>
            {
                schemaVersionFoundRule,
                schemaVersionSupportedRule,
            }
        ) { }

    public Task<(bool fatal, List<ValidationError> errors)> ValidateAsync(RawBatch rawBatch)
    {
        return ValidateSequential(rawBatch);
    }
}
