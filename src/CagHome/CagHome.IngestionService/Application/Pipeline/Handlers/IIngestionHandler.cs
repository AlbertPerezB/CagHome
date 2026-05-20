namespace CagHome.IngestionService.Application.Pipeline.Handlers;

/// <summary>
/// Defines a single step in the ingestion pipeline.
/// </summary>
public interface IIngestionHandler
{
    /// <summary>
    /// Sets the next handler in the chain.
    /// </summary>
    /// <param name="next"> A reference to the next handler. </param>
    /// <returns></returns>
    IIngestionHandler SetNext(IIngestionHandler next);

    /// <summary>
    /// Function determining whether this handler should process the given context.
    /// If it returns false, the context will be passed to the next handler without processing.
    /// </summary>
    /// <param name="context">The ingestion context to be processed.</param>
    /// <returns>True if the handler should process the context; otherwise, false.</returns>
    bool ShouldProcess(IngestionContext context);

    /// <summary>
    /// Processes the context and passes it to the next handler in the chain.
    /// </summary>
    Task HandleAsync(IngestionContext context);
}
