using CagHome.Contracts;
using CagHome.NotificationService;
using CagHome.NotificationService.Infrastructure;
using Microsoft.AspNetCore.Http;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.UseWolverine(options =>
{
    options
        .UseRabbitMqUsingNamedConnection("rabbitmq-broker")
        .AutoProvision()
        .UseConventionalRouting();

    options.Policies.DisableConventionalLocalRouting();

    options
        .Policies.OnException<HttpRequestException>()
        .RetryWithCooldown(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15)
        )
        .Then.MoveToErrorQueue();

    options.Policies.OnException<BadHttpRequestException>().MoveToErrorQueue();

    options
        .Policies.OnAnyException()
        .RetryWithCooldown(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
        .Then.MoveToErrorQueue();

    options.ListenToRabbitQueue("notification.hospital-alert");
    options.ListenToRabbitQueue("notification.patient-alert");
    options.ListenToRabbitQueue("notification.clinician-response");
});

builder.AddMongoDBClient(connectionName: "notificiation-audit");

builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource("Wolverine").AddSource("RabbitMQ.Client"));

builder.Services.AddHttpClient(
    "mock-ehr",
    client =>
    {
        client.BaseAddress = new Uri("https://mock-ehr");
    }
);

builder.Services.AddSingleton<MqttConnectionService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttConnectionService>());
builder.Services.AddSingleton<IMqttPublisher, MqttPublisher>();

var host = builder.Build();
host.Run();
