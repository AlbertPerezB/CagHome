using CagHome.Contracts;
using Wolverine;

public class PatientCacheWarmupService : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly TaskCompletionSource _ready = new();
    private readonly ILogger<PatientCacheWarmupService> _logger;

    public Task WhenReady => _ready.Task;

    public void Complete() => _ready.TrySetResult();

    public PatientCacheWarmupService(
        IMessageBus messageBus,
        ILogger<PatientCacheWarmupService> logger
    )
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Give Wolverine time to start — it's also a hosted service
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        _logger.LogDebug("Cache warm-up: requesting all patient statuses");
        await _messageBus.PublishAsync(new AllPatientStatusesRequested());

        var timeout = Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        var completed = await Task.WhenAny(WhenReady, timeout);

        if (completed == timeout)
        {
            _logger.LogWarning(
                "Cache warm-up timed out after 30 seconds — starting with empty cache"
            );
            _ready.TrySetResult();
        }

        _logger.LogDebug("Cache warm-up complete");
    }
}
