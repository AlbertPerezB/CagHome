using System.Text.Json;
using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class ParseJsonHandler : IngestionHandler
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    protected override Task ProcessAsync(IngestionContext context)
    {
        try
        {
            context.Json = JsonDocument.Parse(context.RawBatch.Payload);
            context.BatchDto = context.Json.RootElement.Deserialize<BatchDto>(_options);
        }
        catch (Exception ex)
        {
            context.FatalError = new ValidationError(ValidationCode.ParseError, ex.Message);
        }

        return Task.CompletedTask;
    }
}
