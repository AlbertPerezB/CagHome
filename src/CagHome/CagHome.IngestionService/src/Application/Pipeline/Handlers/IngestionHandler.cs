namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public abstract class IngestionHandler : IIngestionHandler
{
    private IIngestionHandler? _next;
    protected readonly ILogger _logger;

    protected IngestionHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public IIngestionHandler SetNext(IIngestionHandler next)
    {
        _next = next;
        return next;
    }

    public async Task HandleAsync(IngestionContext context)
    {
        if (ShouldProcess(context))
            await ProcessAsync(context);

        if (_next != null)
            await _next.HandleAsync(context);
    }

    public virtual bool ShouldProcess(IngestionContext context) => context.FatalError == null;

    protected abstract Task ProcessAsync(IngestionContext context);
}
