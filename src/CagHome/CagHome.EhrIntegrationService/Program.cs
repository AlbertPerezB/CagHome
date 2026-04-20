using CagHome.Contracts;
using CagHome.EhrIntegrationService.Application.Pollers;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

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

builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource("Wolverine"));

builder.Services.AddWolverine(options =>
{
    options.UseRabbitMqUsingNamedConnection("messaging").AutoProvision();

    options.ListenToRabbitQueue("ehr.hospital-alerts");

    options
        .PublishMessage<ClinicianResponseReceived>()
        .ToRabbitQueue("notification.clinician-response");

    options.PublishMessage<PatientRegistered>().ToRabbitQueue("patient-registry.new-patient");
});

var host = builder.Build();
await host.RunAsync();
