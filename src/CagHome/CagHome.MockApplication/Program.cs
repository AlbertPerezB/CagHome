using CagHome.MockApplication;
using CagHome.MockApplication.Application;
using CagHome.MockApplication.Domain.Models;
using CagHome.MockApplication.Domain.Profiles;
using CagHome.MockApplication.Infrastructure;
using CagHome.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var registerPatients = builder
    .Configuration.GetSection(SimulatorOptions.SectionName)
    .GetValue<bool>("RegisterPatients");

builder.Services.Configure<SimulatorOptions>(
    builder.Configuration.GetSection(SimulatorOptions.SectionName)
);
builder.Services.AddSingleton<ISimulationProfile, NormalSimulationProfile>();
builder.Services.AddSingleton<ISimulationProfile, ExerciseSimulationProfile>();
builder.Services.AddSingleton<ISimulationProfile, ArrhythmiaSimulationProfile>();
if (registerPatients)
{
    builder.Services.AddHttpClient(
        "mock-ehr",
        client =>
            client.BaseAddress = new Uri(
                builder.Configuration["PopulationSimulator:MockEhrBaseUrl"] ?? "http://mock-ehr"
            )
    );

    builder.Services.AddSingleton<PatientRegistrationService>();
}

builder.Services.AddHostedService<MockApplicationService>();
builder.Services.AddSingleton<IInjectedTelemetryPublisher, InjectedTelemetryPublisher>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost(
    "/simulator/inject",
    async (
        InjectTelemetryRequest request,
        IInjectedTelemetryPublisher injectedTelemetryPublisher,
        ILogger<Program> logger,
        CancellationToken cancellationToken
    ) =>
    {
        if (request.PatientId == Guid.Empty)
        {
            return Results.BadRequest(new { error = "patientId must be a non-empty GUID." });
        }

        if (string.IsNullOrWhiteSpace(request.AppVersion))
        {
            return Results.BadRequest(new { error = "appVersion is required." });
        }

        if (request.Measurements is null || request.Measurements.Count == 0)
        {
            return Results.BadRequest(
                new { error = "measurements must contain at least one item." }
            );
        }

        var correlationId = Guid.NewGuid();
        var batchPayload = new MeasurementBatchPayload(
            AppVersion: request.AppVersion,
            CorrelationId: correlationId,
            Measurements: request.Measurements,
            PatientId: request.PatientId,
            SchemaVersion: request.SchemaVersion
        );

        await injectedTelemetryPublisher.PublishAsync(
            batchPayload,
            request.PatientId,
            cancellationToken
        );

        logger.LogInformation(
            "Injected telemetry for patient {PatientId} with correlation {CorrelationId}",
            request.PatientId,
            correlationId
        );

        return Results.Ok(new InjectTelemetryResponse(correlationId));
    }
);

await app.RunAsync();
