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
using CagHome.IngestionService.Infrastructure.Messaging;
using CagHome.IngestionService.Infrastructure.Schemas;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

//Infrastructure
builder.Services.AddHostedService<MqttConsumerService>();
builder.Services.AddScoped<RabbitMqPublisher>();
builder.Services.AddScoped<MqttPublisher>();
builder.Services.AddSingleton<IJsonSchemaRegistry, JsonSchemaRegistry>();

//Handlers
builder.Services.AddScoped<StructuralValidationHandler>();
builder.Services.AddScoped<ParseJsonHandler>();
builder.Services.AddScoped<BatchValidationHandler>();
builder.Services.AddScoped<TopicValidationHandler>();
builder.Services.AddScoped<MeasurementValidationHandler>();
builder.Services.AddScoped<PublishBatchHandler>();
builder.Services.AddScoped<ErrorPublishingHandler>();
builder.Services.AddScoped<BatchMappingHandler>();
builder.Services.AddScoped<DeserializationHandler>();

//Validators
builder.Services.AddScoped<StructuralValidator>();
builder.Services.AddScoped<BatchValidator>();
builder.Services.AddScoped<MeasurementValidator>();

// Structural rules
builder.Services.AddScoped<IValidationRule<JsonDocument>, SchemaValidationRule>();

// Batch rules
builder.Services.AddScoped<IBatchValidationRule, PatientActiveRule>();

// Measurement rules
builder.Services.AddScoped<IValidationRule<Measurement>, CorrectUnitRule>();
builder.Services.AddScoped<IValidationRule<Measurement>, DeviceReportedNotInFutureRule>();

builder.Services.AddScoped<IIngestionService, IngestionService>();

builder.Services.AddScoped(sp =>
{
    var structural = sp.GetRequiredService<StructuralValidationHandler>();
    var jsonParser = sp.GetRequiredService<ParseJsonHandler>();
    var batchMapping = sp.GetRequiredService<BatchMappingHandler>();
    var deserialization = sp.GetRequiredService<DeserializationHandler>();
    var topicValidation = sp.GetRequiredService<TopicValidationHandler>();
    var batch = sp.GetRequiredService<BatchValidationHandler>();
    var measurement = sp.GetRequiredService<MeasurementValidationHandler>();
    var publish = sp.GetRequiredService<PublishBatchHandler>();
    var errors = sp.GetRequiredService<ErrorPublishingHandler>();

    return IngestionPipelineBuilder.Build(
        structural,
        jsonParser,
        deserialization,
        batchMapping,
        topicValidation,
        batch,
        measurement,
        publish,
        errors
    );
});

builder.Services.AddWolverine(options =>
{
    options.UseRabbitMqUsingNamedConnection("messaging").AutoProvision().UseConventionalRouting();

    options.Policies.DisableConventionalLocalRouting();

    options.ListenToRabbitQueue(PingPongTopology.QueueName);
    options.PublishMessage<PingMessage>().ToRabbitQueue(PingPongTopology.QueueName);
    options.PublishMessage<PongMessage>().ToRabbitQueue(PingPongTopology.QueueName);
});

builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource("Wolverine").AddSource("RabbitMQ.Client"));

var host = builder.Build();

await host.RunAsync();
