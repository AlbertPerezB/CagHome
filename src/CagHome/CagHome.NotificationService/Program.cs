using CagHome.NotificationService;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.UseWolverine(options =>
{
    options.UseRabbitMqUsingNamedConnection("messaging").AutoProvision().UseConventionalRouting();

    options.Policies.DisableConventionalLocalRouting();
});

builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource("Wolverine").AddSource("RabbitMQ.Client"));

var host = builder.Build();
host.Run();
