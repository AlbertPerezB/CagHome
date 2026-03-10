using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MqttConsumerService>();
builder.Services.AddScoped<IngestionPipeline>();
builder.Services.AddScoped<BatchParser>();
builder.Services.AddScoped<BatchValidator>();
builder.Services.AddScoped<MeasurementValidator>();

builder.AddMongoDBClient(connectionName: "mongodb");

var host = builder.Build();

await host.RunAsync();
