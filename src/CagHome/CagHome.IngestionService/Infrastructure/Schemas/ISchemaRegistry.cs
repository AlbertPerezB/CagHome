using Json.Schema;

namespace CagHome.IngestionService.Infrastructure.Schemas;

/// <summary>
/// Resolves JSON schemas by version number for structural validation of incoming payloads.
/// </summary>
public interface IJsonSchemaRegistry
{
    JsonSchema GetSchema(int version);
    bool IsSupported(int version);
}
