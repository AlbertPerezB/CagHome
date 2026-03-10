using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Infrastructure;
using CagHome.IngestionService.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MqttConsumerService>();
builder.Services.AddScoped<IngestionPipeline>();
builder.Services.AddScoped<BatchParser>();
builder.Services.AddScoped<BatchValidator>();
builder.Services.AddScoped<MeasurementValidator>();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<PingPongStartupService>();

builder.AddMongoDBClient(connectionName: "mongodb");

builder.Services.AddWolverine(options =>
{
	options.UseRabbitMqUsingNamedConnection("messaging").AutoProvision();
	options.ListenToRabbitQueue(PingPongTopology.QueueName);
	options.PublishMessage<PingMessage>().ToRabbitQueue(PingPongTopology.QueueName);
	options.PublishMessage<PongMessage>().ToRabbitQueue(PingPongTopology.QueueName);
});

var host = builder.Build();

await host.RunAsync();
