using CagHome.NotificationService;
using Microsoft.AspNetCore.Http;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.UseWolverine(options =>
{
    options.UseRabbitMqUsingNamedConnection("messaging").AutoProvision().UseConventionalRouting();

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
});

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
