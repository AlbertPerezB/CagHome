namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public abstract class IngestionHandler : IIngestionHandler
{
    private IIngestionHandler? _next;

    public IIngestionHandler SetNext(IIngestionHandler next)
    {
        _next = next;
        return next;
    }

    public async Task HandleAsync(IngestionContext context)
    {
        await ProcessAsync(context);

        if (context.FatalError == null && _next != null)
            await _next.HandleAsync(context);
    }

    protected abstract Task ProcessAsync(IngestionContext context);
}
