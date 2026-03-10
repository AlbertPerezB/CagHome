using System.Text.Json;
using CagHome.IngestionService.Domain.Models;

public class BatchParser
{
    public Batch? Parse(string rawPayload)
    {
        var version = ReadSchemaVersion(rawPayload);

        return version switch
        {
            1 => ParseV1(rawPayload),
            _ => null,
        };
    }

    private int? ReadSchemaVersion(string rawPayload)
    {
        using var doc = JsonDocument.Parse(rawPayload);
        if (doc.RootElement.TryGetProperty("schemaVersion", out var v))
            return v.GetInt32();
        return null;
    }

    private Batch? ParseV1(string rawPayload)
    {
        return JsonSerializer.Deserialize<Batch>(rawPayload);
    }
}
