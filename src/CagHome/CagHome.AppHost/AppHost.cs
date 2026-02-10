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



var apiService = builder.AddProject<Projects.CagHome_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.CagHome_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService);

var brokerPort = builder.AddParameter("mqtt-broker-port", "1883");

var broker = builder.AddProject<Projects.CagHome_Broker>("broker")
    .WithEnvironment("MQTT_PORT", brokerPort);

builder.AddProject<Projects.CagHome_Consumer>("consumer")
    .WithReference(broker)
    .WithReference(mongodb)
    .WithEnvironment("MQTT_BROKER_PORT", brokerPort)
    .WithReplicas(3);
    
builder.Build().Run();
