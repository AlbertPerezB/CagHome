using CagHome.IngestionService.Application.Pipeline.Handlers;

public class NoOpHandler : IngestionHandler
{
    protected override Task ProcessAsync(IngestionContext context) => Task.CompletedTask;
}
