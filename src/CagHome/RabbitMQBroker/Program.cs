using RabbitMQBroker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddRabbitMQClient(connectionName: "messaging");
builder.Services.AddHostedService<RabbitMqBrokerWorker>();

var host = builder.Build();

await host.RunAsync();
