using CagHome.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
                .WithHostPort(5432)
                .WithImage("postgres", "18")
                .WithV18DataVolume()
                .WithPgAdmin();

var patientdb = postgres.AddDatabase("patient");

var mongo = builder.AddMongoDB("mongo")
                   .WithLifetime(ContainerLifetime.Persistent);

                   var mongodb = mongo.AddDatabase("mongodb");

var rabbitmq = builder.AddRabbitMQ("messaging");



var apiService = builder.AddProject<Projects.CagHome_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.CagHome_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService);

var brokerPort = builder.AddParameter("mqtt-broker-port", "1883");
var brokerHost = builder.AddParameter("mqtt-broker-host", "localhost");

var broker = builder.AddProject<Projects.CagHome_Broker>("broker")
    .WithEnvironment("MQTT_PORT", brokerPort);

builder.AddProject<Projects.CagHome_IngestionService>("ingestionservice")
    .WithReference(broker)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(mongodb)
    .WithEnvironment("MQTT_BROKER_PORT", brokerPort);
    // .WithReplicas(3);

builder.AddProject<Projects.CagHome_Simulator>("simulator")
    .WithReference(broker)
    .WithEnvironment("Simulator__BrokerHost", brokerHost)
    .WithEnvironment("Simulator__BrokerPort", brokerPort);

builder.AddProject<Projects.RabbitMQBroker>("rabbitmqbroker")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);
    
builder.Build().Run();
