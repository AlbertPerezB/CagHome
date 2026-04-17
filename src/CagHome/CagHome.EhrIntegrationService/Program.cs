using CagHome.Contracts;
using CagHome.EhrIntegrationService.Infrastructure;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

// HTTP client for calling Mock EHR endpoints
builder.Services.AddHttpClient(
    "mock-ehr",
    client =>
    {
        client.BaseAddress = new Uri("https://mock-ehr");
    }
);

// Polling background services
builder.Services.AddHostedService<ClinicianResponsePoller>();
builder.Services.AddHostedService<PatientRegistrationPoller>();

// OpenTelemetry — match the pattern from Ingestion
builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource("Wolverine"));

builder.Services.AddWolverine(options =>
{
    options.UseRabbitMqUsingNamedConnection("messaging").AutoProvision();

    // Consume alert requests from Monitoring
    options.ListenToRabbitQueue("ehr.hospital-alerts");

    // Publish events for downstream consumers (Notification, Patient Registry)
    options
        .PublishMessage<ClinicianResponseReceived>()
        .ToRabbitQueue("notification.clinician-response");

    options.PublishMessage<PatientRegistered>().ToRabbitQueue("patient-registry.new-patient");
});

var host = builder.Build();
await host.RunAsync();
