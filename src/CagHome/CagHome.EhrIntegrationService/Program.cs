using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.EhrIntegrationService.Application.Pollers;
using CagHome.EhrIntegrationService.Infrastructure;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient(
    "mock-ehr",
    client =>
    {
        client.BaseAddress = new Uri("https://mock-ehr");
    }
);
builder.AddServiceDefaults();
builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing =>
        tracing
            .AddSource("Wolverine")
            .AddSource("RabbitMQ.Client")
            .AddSource("CagHome.EhrIntegrationService")
    );

// Polling background services
builder.Services.AddHostedService<ClinicianResponsePoller>();
builder.Services.AddHostedService<PatientRegistrationPoller>();

builder.Services.AddWolverine(options =>
{
    options.UseRabbitMqUsingNamedConnection("rabbitmq-broker").AutoProvision();

    options
        .PublishMessage<ClinicianResponseReceived>()
        .ToRabbitQueue("notification.clinician-response");

    options
        .PublishMessage<PatientStatusUpdateRequested>()
        .ToRabbitQueue("patient-registry.patient-status-update");

    options.PublishMessage<CareplanUpdateRequested>().ToRabbitQueue("monitoring.careplan-update");

    options.Policies.OnAnyException().MoveToErrorQueue();
});

builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource("Wolverine").AddSource("RabbitMQ.Client"));
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

var host = builder.Build();
await host.RunAsync();
