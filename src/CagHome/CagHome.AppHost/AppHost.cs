var builder = DistributedApplication.CreateBuilder(args);

var mongo = builder.AddMongoDB("mongo").WithLifetime(ContainerLifetime.Persistent);
var patientregistryDb = mongo.AddDatabase("patient-registry");
var notificationAuditDb = mongo.AddDatabase("notification-audit");
var monitoringAuditDb = mongo.AddDatabase("monitoring-audit");
var monitoringConfigDb = mongo.AddDatabase("monitoring-config");
var errorDb = mongo.AddDatabase("errors");

var redis = builder.AddRedis("patient-cache");

var rabbitmqBroker = builder.AddRabbitMQ("rabbitmq-broker").WithManagementPlugin();

var brokerPort = builder.AddParameter("mqtt-broker-port", "1883");
var brokerHost = builder.AddParameter("mqtt-broker-host", "localhost");

var mqttBroker = builder
    .AddProject<Projects.CagHome_MqttBroker>("mqtt-broker")
    .WithEnvironment("MQTT_PORT", brokerPort);

builder
    .AddProject<Projects.CagHome_IngestionService>("ingestionservice")
    .WithReference(mqttBroker)
    .WithReference(rabbitmqBroker)
    .WithReference(redis)
    .WaitFor(rabbitmqBroker)
    .WithEnvironment("MQTT_BROKER_PORT", brokerPort);

// .WithReplicas(3);

builder
    .AddProject<Projects.CagHome_Simulator>("simulator")
    .WithReference(mqttBroker)
    .WithEnvironment("Simulator__BrokerHost", brokerHost)
    .WithEnvironment("Simulator__BrokerPort", brokerPort);

builder
    .AddProject<Projects.CagHome_RabbitMQBroker>("rabbitmqbroker")
    .WithReference(rabbitmqBroker)
    .WaitFor(rabbitmqBroker);

var mockEhr = builder.AddProject<Projects.CagHome_MockEhr>("mock-ehr");

builder
    .AddProject<Projects.CagHome_NotificationService>("notification")
    .WithReference(rabbitmqBroker)
    .WithReference(mockEhr)
    .WithReference(notificationAuditDb)
    .WithReference(mqttBroker)
    .WithEnvironment("MQTT_BROKER_PORT", brokerPort)
    .WaitFor(rabbitmqBroker);

var ehrIntegration = builder
    .AddProject<Projects.CagHome_EhrIntegrationService>("ehr-integration")
    .WithReference(rabbitmqBroker)
    .WithReference(mockEhr)
    .WaitFor(rabbitmqBroker);

builder
    .AddProject<Projects.CagHome_MonitoringService>("monitoring")
    .WithReference(rabbitmqBroker)
    .WithReference(monitoringAuditDb)
    .WaitFor(rabbitmqBroker);

builder
    .AddProject<Projects.CagHome_ErrorHandler>("caghome-errorhandler")
    .WithReference(rabbitmqBroker)
    .WithReference(errorDb)
    .WaitFor(rabbitmqBroker);

builder
    .AddProject<Projects.CagHome_PatientRegistryService>("caghome-patientregistryservice")
    .WithReference(rabbitmqBroker)
    .WithReference(patientregistryDb)
    .WaitFor(rabbitmqBroker);

builder.Build().Run();
