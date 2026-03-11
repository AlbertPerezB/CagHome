using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Schemas;

namespace CagHome.IngestionService.Application.Validation.StructuralValidation;

public class SchemaVersionSupportedRule : IValidationRule<RawBatch>
{
    private readonly IJsonSchemaRegistry _registry;

    public bool IsFatal => true;

    public SchemaVersionSupportedRule(IJsonSchemaRegistry registry)
    {
        _registry = registry;
    }

    public Task<ValidationError?> ValidateAsync(RawBatch batch)
    {
        var version = batch.JsonPayload.GetProperty("schemaVersion").GetInt32();
        if (!_registry.IsSupported(version))
        {
            return Task.FromResult<ValidationError?>(
                new ValidationError(
                    ValidationCode.UnsupportedSchemaVersion,
                    $"Unsupported schema version {version}"
                )
            );
        }

        return Task.FromResult<ValidationError?>(null);
    }
}
