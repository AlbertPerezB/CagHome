using System.Text.Json;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

/// <summary>
/// Handler responsible for parsing the raw json payload into a JsonDocument and
/// storing it in the context for downstream handlers to use.
/// </summary>
public class ParseJsonHandler(ILogger<ParseJsonHandler> logger) : IngestionHandler
{
    protected override Task ProcessAsync(IngestionContext context)
    {
        logger.LogDebug("Parsing json");
        try
        {
            context.Json = JsonDocument.Parse(context.RawBatch.Payload);
        }
        catch (Exception ex)
        {
            context.FatalError = new ValidationError(ValidationCode.ParseError, ex.Message);
        }

        return Task.CompletedTask;
    }
}
