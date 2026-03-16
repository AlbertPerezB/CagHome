using System.Text.Json;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.StructuralValidation;

public class SchemaVersionFoundRule : IValidationRule<JsonDocument>
{
    public bool IsFatal => true;

    public Task<ValidationError?> ValidateAsync(JsonDocument json)
    {
        var version = GetVersionFromJson(json.RootElement);
        if (version == null)
        {
            return Task.FromResult<ValidationError?>(
                new ValidationError(
                    ValidationCode.UnsupportedSchemaVersion,
                    $"No field corresponding to schema version"
                )
            );
        }

        return Task.FromResult<ValidationError?>(null);
    }

    private int? GetVersionFromJson(JsonElement json)
    {
        var success = json.TryGetProperty("schemaVersion", out var version);
        return success ? version.GetInt32() : null;
    }
}
