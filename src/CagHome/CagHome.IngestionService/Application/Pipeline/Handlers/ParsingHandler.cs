using System.Text.Json;
using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class ParsingHandler : IngestionHandler
{
    private Batch? Parse(JsonElement jsonElement)
    {
        return JsonSerializer.Deserialize<Batch>(jsonElement.GetRawText());
    }

    protected override Task ProcessAsync(IngestionContext context)
    {
        try
        {
            context.Batch = Parse(context.RawBatch.JsonPayload);
        }
        catch (Exception ex)
        {
            context.fatalError = new ValidationError(ValidationCode.ParseError, ex.Message);
        }
        return Task.CompletedTask;
    }
}
