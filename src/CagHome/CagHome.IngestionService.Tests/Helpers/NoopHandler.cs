using CagHome.IngestionService.Application.Pipeline.Handlers;
using Microsoft.Extensions.Logging;

public class NoOpHandler : IngestionHandler
{
    public NoOpHandler(ILoggerFactory loggerFactory)
        : base(loggerFactory) { }

    protected override Task ProcessAsync(IngestionContext context) => Task.CompletedTask;
}
