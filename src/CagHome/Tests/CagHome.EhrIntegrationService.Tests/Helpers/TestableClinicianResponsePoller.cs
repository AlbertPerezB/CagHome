using CagHome.EhrIntegrationService.Application.Pollers;
using CagHome.EhrIntegrationService.Infrastructure;
using Microsoft.Extensions.Logging;

/// <summary>
/// A testable version of the ClinicianResponsePoller that exposes the ExecuteAsync method for testing purposes.
/// </summary>
public class TestableClinicianResponsePoller : ClinicianResponsePoller
{
    public TestableClinicianResponsePoller(
        IHttpClientFactory httpClientFactory,
        ILogger<ClinicianResponsePoller> logger,
        IRabbitMqPublisher publisher
    )
        : base(httpClientFactory, logger, publisher) { }

    public new Task ExecuteAsync(CancellationToken ct) => base.ExecuteAsync(ct);
}
