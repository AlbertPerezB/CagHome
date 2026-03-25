using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CagHome.IngestionService.Tests.Pipeline;

// Lets tests verify the chain halts by injecting a spy as the next handler
public class DelegateHandler : IngestionHandler
{
    private readonly Func<Task> _action;

    public DelegateHandler(Func<Task> action)
        : base(NullLoggerFactory.Instance)
    {
        _action = action;
    }

    protected override Task ProcessAsync(IngestionContext context) => _action();
}
