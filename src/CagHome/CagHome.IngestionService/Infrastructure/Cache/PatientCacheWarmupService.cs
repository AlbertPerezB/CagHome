using CagHome.Contracts;
using Wolverine;

/// <summary>
/// Service responsible for warming up the patient cache by requesting all patient statuses
/// from the message bus and populating the local cache.
/// </summary>
public class PatientCacheWarmupService : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly TaskCompletionSource _ready = new();
    private readonly ILogger<PatientCacheWarmupService> _logger;
    private readonly int _maxRetries = 5;

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
        for (int i = 0; i < _maxRetries; i++)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                _logger.LogInformation(
                    "Cache warm-up: requesting all patient statuses (attempt {Attempt})",
                    i + 1
                );
                await _messageBus.PublishAsync(new AllPatientStatusesRequested());
                break;
            }
            catch (InvalidOperationException) when (i < _maxRetries - 1)
            {
                _logger.LogWarning("RabbitMQ not ready yet, retrying...");
            }
        }

        var timeout = Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        var completed = await Task.WhenAny(WhenReady, timeout);

        if (completed == timeout)
        {
            _logger.LogWarning(
                "Cache warm-up timed out after 30 seconds — starting with empty cache"
            );
            _ready.TrySetResult();
        }

        _logger.LogInformation("Cache warm-up complete");
    }
}
