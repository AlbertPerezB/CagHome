using System.Text.Json;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class ParseJsonHandler : IngestionHandler
{
    public ParseJsonHandler(ILoggerFactory loggerFactory)
        : base(loggerFactory) { }

    protected override Task ProcessAsync(IngestionContext context)
    {
        _logger.LogInformation("Parsing json");
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
