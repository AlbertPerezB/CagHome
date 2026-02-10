using CagHome.Consumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MqttConsumerService>();

builder.AddMongoDBClient(connectionName: "mongodb");

var host = builder.Build();

await host.RunAsync();
