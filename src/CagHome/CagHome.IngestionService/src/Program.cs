using System.Text.Json;
using CagHome.IngestionService.Application;
using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Application.Validation.BatchValidation;
using CagHome.IngestionService.Application.Validation.MeasurementValidation;
using CagHome.IngestionService.Application.Validation.StructuralValidation;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure;
using CagHome.IngestionService.Infrastructure.Schemas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MqttConsumerService>();
builder.Services.AddScoped<RabbitMqPublisher>();
builder.Services.AddScoped<MqttPublisher>();

builder.Services.AddSingleton<IJsonSchemaRegistry, JsonSchemaRegistry>();

//Handlers
builder.Services.AddScoped<StructuralValidationHandler>();
builder.Services.AddScoped<ParsingHandler>();
builder.Services.AddScoped<BatchValidationHandler>();
builder.Services.AddScoped<MeasurementValidationHandler>();
builder.Services.AddScoped<PublishBatchHandler>();
builder.Services.AddScoped<ErrorPublishingHandler>();

//Validators
builder.Services.AddScoped<StructuralValidator>();
builder.Services.AddScoped<BatchValidator>();
builder.Services.AddScoped<MeasurementValidator>();

// Structural rules
builder.Services.AddScoped<IValidationRule<JsonDocument>, SchemaVersionFoundRule>();
builder.Services.AddScoped<IValidationRule<JsonDocument>, SchemaVersionSupportedRule>();

// Batch rules
builder.Services.AddScoped<IBatchValidationRule, PatientActiveRule>();

// Measurement rules
builder.Services.AddScoped<IValidationRule<Measurement>, CorrectUnitRule>();
builder.Services.AddScoped<IValidationRule<Measurement>, DeviceReportedNotInFutureRule>();

builder.Services.AddScoped<IIngestionService, IngestionService>();

builder.Services.AddScoped<IIngestionHandler>(sp =>
{
    var structural = sp.GetRequiredService<StructuralValidationHandler>();
    var parsing = sp.GetRequiredService<ParsingHandler>();
    var batch = sp.GetRequiredService<BatchValidationHandler>();
    var measurement = sp.GetRequiredService<MeasurementValidationHandler>();
    var publish = sp.GetRequiredService<PublishBatchHandler>();
    var errors = sp.GetRequiredService<ErrorPublishingHandler>();

    return IngestionPipelineBuilder.Build(structural, parsing, batch, measurement, publish, errors);
});
builder.AddMongoDBClient(connectionName: "mongodb");

var host = builder.Build();

await host.RunAsync();
