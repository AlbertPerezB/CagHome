using CagHome.MockEhr;
using CagHome.MockEhr.Domain;
using CagHome.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry();
builder.Services.AddSingleton<MockEhrStore>();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapDefaultEndpoints();

// POST /alerts
// Receives alerts from our system. Returns 202 Accepted
app.MapPost(
    "/alerts",
    (AlertDTO alert, MockEhrStore store, ILogger<Program> logger) =>
    {
        var received = new ReceivedAlert(
            AlertId: alert.AlertId,
            CorrelationId: alert.CorrelationId,
            PatientId: alert.PatientId,
            HospitalId: alert.HospitalId,
            Message: alert.Message,
            Severity: (Severity)alert.Severity,
            ReceivedAtUtc: DateTime.UtcNow
        );

        store.Alerts.Add(received);

        logger.LogInformation(
            "Alert received: AlertId={AlertId}, PatientId={PatientId}, Message={Message}",
            received.AlertId,
            received.PatientId,
            received.Message
        );

        return Results.Accepted(value: new { received.AlertId });
    }
);

// GET /clinician-responses?since={ISO 8601 timestamp}
// Returns clinician responses created after the given timestamp.
app.MapGet(
    "/clinician-responses",
    (DateTime? since, MockEhrStore store) =>
    {
        var cutoff = since ?? DateTime.MinValue;

        var responses = store
            .ClinicianResponses.Where(r => r.CreatedAtUtc > cutoff)
            .OrderBy(r => r.CreatedAtUtc)
            .ToList();

        return Results.Ok(responses);
    }
);

// GET /patients?since={ISO 8601 timestamp}
// Returns patient registrations created after the given timestamp.
app.MapGet(
    "/patients",
    (DateTime? since, MockEhrStore store) =>
    {
        var cutoff = since ?? DateTime.MinValue;

        var patients = store
            .PatientRegistrations.Where(p => p.UpdatedAtUtc > cutoff)
            .OrderBy(p => p.UpdatedAtUtc)
            .ToList();

        return Results.Ok(patients);
    }
);

// MOCK-ONLY ENDPOINTS
// These exist solely so we can simulate hospital activity
// during development and demos.

// POST /mock/clinician-response
// Simulates a clinician typing a response to an alert.
app.MapPost(
    "/mock/clinician-response",
    (ClinicianResponseDto response, MockEhrStore store, ILogger<Program> logger) =>
    {
        try
        {
            var alert = store.Alerts.FirstOrDefault(a => a.AlertId == response.AlertId);
            if (alert == null)
            {
                return Results.NotFound("No alert found with the given AlertId");
            }

            var responseId = Guid.NewGuid();
            var clinicianResponse = new ClinicianResponse(
                AlertId: response.AlertId,
                CreatedAtUtc: response.CreatedAtUtc,
                CorrelationId: alert.CorrelationId,
                HospitalId: response.HospitalId,
                Message: response.Message,
                PatientId: response.PatientId,
                ResponseId: responseId
            );
            store.ClinicianResponses.Enqueue(clinicianResponse);

            logger.LogInformation(
                "Mock: clinician response created: ResponseId={ResponseId}, AlertId={AlertId}",
                response.AlertId,
                responseId
            );
            return Results.Created(
                $"/clinician-responses/{clinicianResponse.ResponseId}",
                new CorrelationIdResponse(alert.CorrelationId)
            );
        }
        catch (Exception ex)
        {
            return Results.InternalServerError(ex.Message);
        }
    }
);

// POST /mock/patient
// Simulates a new patient being registered in the hospital system.
app.MapPost(
    "/mock/patient",
    (PatientRegistrationUpdate request, MockEhrStore store, ILogger<Program> logger) =>
    {
        store.PatientRegistrations.Enqueue(request);

        logger.LogInformation(
            "Mock: patient registered: PatientId={PatientId}, Name={Name}",
            request.PatientId,
            request.Careplan
        );

        return Results.Created($"/patients/{request.PatientId}", request);
    }
);

// GET /mock/alerts
// View all received alerts, useful for verifying that alerts arrived.
app.MapGet(
    "/mock/alerts",
    (MockEhrStore store) =>
    {
        return Results.Ok(store.Alerts.OrderByDescending(a => a.ReceivedAtUtc).ToList());
    }
);

app.Run();
