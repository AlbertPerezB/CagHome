using CagHome.Broker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MqttBrokerService>();

var host = builder.Build();

await host.RunAsync();
