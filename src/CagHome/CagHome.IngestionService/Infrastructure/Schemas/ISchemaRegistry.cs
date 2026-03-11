using Json.Schema;

namespace CagHome.IngestionService.Infrastructure.Schemas;

public interface IJsonSchemaRegistry
{
    JsonSchema GetSchema(int version);
    bool IsSupported(int version);
}
