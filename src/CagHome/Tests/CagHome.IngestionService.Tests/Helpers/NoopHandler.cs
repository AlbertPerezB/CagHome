using CagHome.IngestionService.Application.Pipeline.Handlers;
using Microsoft.Extensions.Logging;

public class NoOpHandler : IngestionHandler
{
    protected override Task ProcessAsync(IngestionContext context) => Task.CompletedTask;
}
