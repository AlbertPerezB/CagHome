using System.Text.Json;
using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class ParsingHandler : IngestionHandler
{
    protected override Task ProcessAsync(IngestionContext context)
    {
        try
        {
            context.Json = JsonDocument.Parse(context.RawBatch.Payload);

            context.BatchDto = JsonSerializer.Deserialize<BatchDto>(
                context.Json.RootElement.GetRawText()
            );
        }
        catch (Exception ex)
        {
            context.FatalError = new ValidationError(ValidationCode.ParseError, ex.Message);
        }

        return Task.CompletedTask;
    }
}
