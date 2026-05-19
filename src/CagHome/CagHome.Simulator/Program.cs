using CagHome.ServiceDefaults;
using CagHome.Simulator;
using CagHome.Simulator.Application;
using CagHome.Simulator.Domain.Models;
using CagHome.Simulator.Domain.Profiles;
using CagHome.Simulator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<SimulatorOptions>(builder.Configuration.GetSection(SimulatorOptions.SectionName));
builder.Services.AddSingleton<ISimulationProfile, NormalSimulationProfile>();
builder.Services.AddSingleton<ISimulationProfile, ExerciseSimulationProfile>();
builder.Services.AddSingleton<ISimulationProfile, ArrhythmiaSimulationProfile>();
builder.Services.AddHostedService<BiometricPublisherService>();
builder.Services.AddSingleton<IInjectedTelemetryPublisher, InjectedTelemetryPublisher>();

var app = builder.Build();

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
			return Results.BadRequest(new { error = "measurements must contain at least one item." });
		}

		var correlationId = Guid.NewGuid();
		var batchPayload = new MeasurementBatchPayload(
			AppVersion: request.AppVersion,
			CorrelationId: correlationId,
			Measurements: request.Measurements,
			PatientId: request.PatientId,
			SchemaVersion: request.SchemaVersion
		);

		await injectedTelemetryPublisher.PublishAsync(batchPayload, request.PatientId, cancellationToken);

		logger.LogInformation(
			"Injected telemetry for patient {PatientId} with correlation {CorrelationId}",
			request.PatientId,
			correlationId
		);

		return Results.Ok(new InjectTelemetryResponse(correlationId));
	}
);

await app.RunAsync();
