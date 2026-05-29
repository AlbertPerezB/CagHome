using CagHome.IngestionService.Application.Pipeline.Handlers;

/// <summary>
/// A no-op ingestion handler that can be used in tests to bypass processing logic when the
/// focus of the test is on other parts of the pipeline.
/// </summary>
public class NoOpHandler : IngestionHandler
{
    protected override Task ProcessAsync(IngestionContext context) => Task.CompletedTask;
}
