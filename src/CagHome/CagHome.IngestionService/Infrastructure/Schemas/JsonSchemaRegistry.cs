using System.Reflection;
using Json.Schema;

namespace CagHome.IngestionService.Infrastructure.Schemas;

public class JsonSchemaRegistry : IJsonSchemaRegistry
{
    private readonly Dictionary<int, JsonSchema> _schemas;

    public JsonSchemaRegistry()
    {
        _schemas = new()
        {
            { 1, LoadEmbeddedSchema("CagHome.IngestionService.Infrastructure.Schemas.v1.json") },
        };
    }

    public JsonSchema GetSchema(int version)
    {
        if (!_schemas.TryGetValue(version, out var schema))
            throw new ArgumentException($"Unsupported schema version {version}");

        return schema;
    }

    public bool IsSupported(int version) => _schemas.ContainsKey(version);

    private static JsonSchema LoadEmbeddedSchema(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Schema resource {resourceName} not found");

        using var reader = new StreamReader(stream);

        return JsonSchema.FromText(reader.ReadToEnd());
    }
}
