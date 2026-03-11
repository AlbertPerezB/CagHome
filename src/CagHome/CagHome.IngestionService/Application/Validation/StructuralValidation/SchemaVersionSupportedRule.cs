using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Schemas;

namespace CagHome.IngestionService.Application.Validation.StructuralValidation;

public class SchemaVersionSupportedRule : IValidationRule<RawBatch>
{
    private readonly JsonSchemaRegistry _registry;

    public bool IsFatal => true;

    public SchemaVersionSupportedRule(JsonSchemaRegistry registry)
    {
        _registry = registry;
    }

    public Task<ValidationError?> ValidateAsync(RawBatch batch)
    {
        var version = batch.RawPayload.GetProperty("schemaVersion").GetInt32();
        if (!_registry.IsSupported(version))
        {
            return Task.FromResult<ValidationError?>(
                new ValidationError(
                    $"Unsupported schema version {version}",
                    ValidationCode.UnsupportedSchemaVersion
                )
            );
        }

        return Task.FromResult<ValidationError?>(null);
    }
}
