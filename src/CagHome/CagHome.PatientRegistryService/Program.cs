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

    options.Policies.OnAnyException().MoveToErrorQueue();

    options.ListenToRabbitQueue("patient-registry.patient-status-update");
});

builder.AddMongoDBClient(connectionName: "patient-registry");

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

var host = builder.Build();
host.Run();
