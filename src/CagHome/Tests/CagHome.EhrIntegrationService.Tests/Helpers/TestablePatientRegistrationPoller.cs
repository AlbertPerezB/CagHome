using CagHome.EhrIntegrationService.Application.Pollers;
using CagHome.EhrIntegrationService.Infrastructure;
using Microsoft.Extensions.Logging;

public class TestablePatientRegistrationPoller : PatientRegistrationPoller
{
    public TestablePatientRegistrationPoller(
        IHttpClientFactory httpClientFactory,
        ILogger<PatientRegistrationPoller> logger,
        IRabbitMqPublisher publisher
    )
        : base(httpClientFactory, logger, publisher) { }

    public new Task ExecuteAsync(CancellationToken ct) => base.ExecuteAsync(ct);
}
