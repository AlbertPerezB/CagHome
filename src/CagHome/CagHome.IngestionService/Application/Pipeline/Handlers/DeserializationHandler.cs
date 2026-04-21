using System.Text.Json;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class DeserializationHandler(ILogger<DeserializationHandler> logger) : IngestionHandler
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    protected override Task ProcessAsync(IngestionContext context)
    {
        logger.LogInformation("Parsing json");
        try
        {
            if (context.Json != null)
            {
                context.BatchDto = context.Json.RootElement.Deserialize<BatchDto>(_options);
            }
        }
        catch (Exception ex)
        {
            context.FatalError = new ValidationError(ValidationCode.ParseError, ex.Message);
        }

        return Task.CompletedTask;
    }
}
