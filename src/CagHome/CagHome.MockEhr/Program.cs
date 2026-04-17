using CagHome.Contracts;
using CagHome.MockEhr;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MockEhrStore>();

var app = builder.Build();

// POST /alerts
// Receives alerts from our system. A real EHR would route this
// to a clinician dashboard or pager system.
app.MapPost(
    "/alerts",
    (HospitalAlertRequested alert, MockEhrStore store, ILogger<Program> logger) =>
    {
        var received = new ReceivedAlert(
            AlertId: Guid.NewGuid(),
            PatientId: alert.PatientId,
            HospitalId: alert.HospitalId,
            Message: alert.Message,
            Severity: alert.Severity,
            ReceivedAtUtc: DateTime.UtcNow
        );

        store.Alerts.Add(received);

        logger.LogInformation(
            "Alert received: AlertId={AlertId}, PatientId={PatientId}, Severity={Severity}",
            received.AlertId,
            received.PatientId,
            received.Severity
        );

        return Results.Accepted(value: new { received.AlertId });
    }
);

// GET /clinician-responses?since={ISO 8601 timestamp}
// Returns clinician responses created after the given timestamp.
// Our EHR Integration Service polls this endpoint.
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
// Our EHR Integration Service polls this endpoint.
app.MapGet(
    "/patients",
    (DateTime? since, MockEhrStore store) =>
    {
        var cutoff = since ?? DateTime.MinValue;

        var patients = store
            .PatientRegistrations.Where(p => p.RegisteredAtUtc > cutoff)
            .OrderBy(p => p.RegisteredAtUtc)
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
    (ClinicianResponseRequest request, MockEhrStore store, ILogger<Program> logger) =>
    {
        var response = new ClinicianResponse(
            ResponseId: Guid.NewGuid(),
            AlertId: request.AlertId,
            PatientId: request.PatientId,
            Message: request.Message,
            CreatedAtUtc: DateTime.UtcNow
        );

        store.ClinicianResponses.Enqueue(response);

        logger.LogInformation(
            "Mock: clinician response created: ResponseId={ResponseId}, AlertId={AlertId}",
            response.ResponseId,
            response.AlertId
        );

        return Results.Created($"/clinician-responses/{response.ResponseId}", response);
    }
);

// POST /mock/patient
// Simulates a new patient being registered in the hospital system.
app.MapPost(
    "/mock/patient",
    (PatientRegistrationRequest request, MockEhrStore store, ILogger<Program> logger) =>
    {
        var patient = new PatientRegistration(
            PatientId: Guid.NewGuid(),
            Name: request.Name,
            RegisteredAtUtc: DateTime.UtcNow
        );

        store.PatientRegistrations.Enqueue(patient);

        logger.LogInformation(
            "Mock: patient registered: PatientId={PatientId}, Name={Name}",
            patient.PatientId,
            patient.Name
        );

        return Results.Created($"/patients/{patient.PatientId}", patient);
    }
);

// GET /mock/alerts
// View all received alerts — useful for verifying that alerts arrived.
app.MapGet(
    "/mock/alerts",
    (MockEhrStore store) =>
    {
        return Results.Ok(store.Alerts.OrderByDescending(a => a.ReceivedAtUtc).ToList());
    }
);

app.Run();

// Request DTOs for mock-only endpoints

public record ClinicianResponseRequest(Guid AlertId, Guid PatientId, string Message);

public record PatientRegistrationRequest(string Name);
