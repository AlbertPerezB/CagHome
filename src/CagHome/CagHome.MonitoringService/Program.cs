using CagHome.Contracts;
using CagHome.MonitoringService.Application.Decision;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Application.Decision.Policies;
using CagHome.MonitoringService.Infrastructure;
using Wolverine;
using Wolverine.RabbitMQ;


var builder = Host.CreateApplicationBuilder(args);

builder.UseWolverine(options =>
{
    options
        .UseRabbitMqUsingNamedConnection("rabbitmq-broker")
        .AutoProvision()
        .UseConventionalRouting();

    options.Policies.DisableConventionalLocalRouting();

    options.ListenToRabbitQueue("monitoring.batch-received");
    options.ListenToRabbitQueue("monitoring.careplan-update");

    options.PublishMessage<PatientAlertRequested>().ToRabbitQueue("notification.patient-alert");
    options.PublishMessage<HospitalAlertRequested>().ToRabbitQueue("notification.hospital-alert");
});

builder.AddMongoDBClient(connectionName: "monitoring-audit");
builder.AddMongoDBClient(connectionName: "monitoring-patientcareplans");

builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource("Wolverine").AddSource("RabbitMQ.Client"));

builder.Services.AddSingleton<ICareplanDecisionPolicy, NoneCareplanPolicy>();
builder.Services.AddSingleton<ICareplanDecisionPolicy, ValveDiseaseCareplanPolicy>();
builder.Services.AddSingleton<ICareplanDecisionPolicy, CoronaryArteryDiseaseCareplanPolicy>();
builder.Services.AddSingleton<ICareplanDecisionPolicy, CardiomyopathyCareplanPolicy>();
builder.Services.AddSingleton<ICareplanPolicyResolver, CareplanPolicyResolver>();
builder.Services.AddSingleton<ICooldownService, InMemoryCooldownService>();
builder.Services.AddSingleton<IPatientCareplanStore, MongoPatientCareplanStore>();
builder.Services.AddSingleton<IDecisionAuditStore, MongoDecisionAuditStore>();
builder.Services.AddHostedService<PolicyResolutionStartupCheckService>();

var host = builder.Build();
host.Run();
