using System.Text.Json;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Schemas;
using Json.Schema;

namespace CagHome.IngestionService.Application.Validation.StructuralValidation;

public class SchemaValidationRule : IValidationRule<JsonDocument>
{
    private readonly IJsonSchemaRegistry _registry;

    public bool IsFatal => true;

    public SchemaValidationRule(IJsonSchemaRegistry registry)
    {
        _registry = registry;
    }

    public Task<ValidationError?> ValidateAsync(JsonDocument json)
    {
        if (!json.RootElement.TryGetProperty("schemaVersion", out var versionElement))
            return Fatal(ValidationCode.MissingRequiredField, "No schemaVersion field found");

        if (versionElement.ValueKind != JsonValueKind.Number)
            return Fatal(ValidationCode.ParseError, "Schema version is not a number");

        versionElement.TryGetInt32(out var version);

        if (!_registry.IsSupported(version))
            return Fatal(
                ValidationCode.UnsupportedSchemaVersion,
                $"Unsupported schema version {version}"
            );

        var schema = _registry.GetSchema(version);
        var result = schema.Evaluate(
            json.RootElement,
            new EvaluationOptions
            {
                OutputFormat = OutputFormat.List,
                RequireFormatValidation = true,
            }
        );

        if (!result.IsValid)
        {
            if (result.Details != null)
            {
                var details =
                    result
                        .Details.Where(d => d.Errors != null)
                        .SelectMany(d => d.Errors!)
                        .Select(e => $"{e.Key}: {e.Value}")
                        .FirstOrDefault()
                    ?? "Schema validation failed";

                return Fatal(ValidationCode.InvalidSchema, details);
            }
        }

        return Task.FromResult<ValidationError?>(null);
    }

    private static Task<ValidationError?> Fatal(ValidationCode code, string message) =>
        Task.FromResult<ValidationError?>(new ValidationError(code, message));
}
