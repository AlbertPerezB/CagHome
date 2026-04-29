using CagHome.ErrorHandler;
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

    options.ListenToRabbitQueue("error-handler.error");
});

builder.AddMongoDBClient(connectionName: "errors");

builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource("Wolverine").AddSource("RabbitMQ.Client"));

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
