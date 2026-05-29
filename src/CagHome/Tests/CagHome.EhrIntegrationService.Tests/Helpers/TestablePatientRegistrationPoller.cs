using CagHome.EhrIntegrationService.Application.Pollers;
using CagHome.EhrIntegrationService.Infrastructure;
using Microsoft.Extensions.Logging;

/// <summary>
/// A testable version of the patient registration poller that overrides the polling interval to 1 second and
/// exposes the ExecuteAsync method for testing purposes.
/// </summary>
public class TestablePatientRegistrationPoller : PatientRegistrationPoller
{
    public TestablePatientRegistrationPoller(
        IHttpClientFactory httpClientFactory,
        ILogger<PatientRegistrationPoller> logger,
        IRabbitMqPublisher publisher
    )
        : base(httpClientFactory, logger, publisher) { }

    protected override int pollingIntervalSeconds => 1;

    public new Task ExecuteAsync(CancellationToken ct) => base.ExecuteAsync(ct);
}
