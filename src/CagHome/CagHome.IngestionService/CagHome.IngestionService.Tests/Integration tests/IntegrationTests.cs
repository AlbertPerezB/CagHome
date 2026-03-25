using System.Text.Json;
using CagHome.IngestionService.Application;
using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Application.Validation.MeasurementValidation;
using CagHome.IngestionService.Application.Validation.StructuralValidation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Schemas;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CagHome.IngestionService.Tests.Integration;

public class IngestionServiceIntegrationTests
{
    private static readonly Guid PatientId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000000");

    private static readonly string ValidTopic = $"biometrics/{PatientId}/telemetry";

    private static string TestPayload() => File.ReadAllText("TestData/test_batch.json");

    private static IIngestionService BuildService(
        IIngestionHandler? publishOverride = null,
        IIngestionHandler? errorOverride = null
    )
    {
        var loggerFactory = NullLoggerFactory.Instance;
        var registry = new JsonSchemaRegistry();

        var parseJson = new ParseJsonHandler(loggerFactory);
        var structuralValidator = new StructuralValidator(
            new List<IValidationRule<JsonDocument>> { new SchemaValidationRule(registry) }
        );
        var structural = new StructuralValidationHandler(structuralValidator, loggerFactory);
        var batchMapping = new BatchMappingHandler(loggerFactory);
        var topicValidation = new TopicValidationHandler(loggerFactory);
        var measurementRules = new List<IValidationRule<Measurement>>
        {
            new CorrectUnitRule(),
            new DeviceReportedNotInFutureRule(),
        };
        var measurementValidator = new MeasurementValidator(measurementRules);
        var measurementValidation = new MeasurementValidationHandler(
            measurementValidator,
            loggerFactory
        );
        var publish = publishOverride ?? new NoOpHandler(loggerFactory);
        var errors = errorOverride ?? new NoOpHandler(loggerFactory);

        parseJson
            .SetNext(structural)
            .SetNext(batchMapping)
            .SetNext(topicValidation)
            .SetNext(measurementValidation)
            .SetNext(publish)
            .SetNext(errors);

        return new Application.IngestionService(parseJson);
    }

    // --- Happy path ---

    [Fact]
    public async Task ValidBatch_PipelineCompletesWithNoFatalError()
    {
        var service = BuildService();
        var raw = new RawBatch(ValidTopic, TestPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.Null(context.FatalError);
    }

    [Fact]
    public async Task ValidBatch_BatchIsMapped()
    {
        var service = BuildService();
        var raw = new RawBatch(ValidTopic, TestPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.NotNull(context.Batch);
        Assert.Equal(PatientId, context.Batch!.PatientId);
        Assert.Equal(1, context.Batch.SchemaVersion);
    }

    [Fact]
    public async Task ValidBatch_AllMeasurementsMapped()
    {
        var service = BuildService();
        var raw = new RawBatch(ValidTopic, TestPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.Equal(13, context.Batch!.Measurements.Count);
    }

    [Fact]
    public async Task ValidBatch_NoMeasurementValidationErrors()
    {
        var service = BuildService();
        var raw = new RawBatch(ValidTopic, TestPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.All(context.Batch!.Measurements, m => Assert.Empty(m.ValidationErrors));
    }

    // --- Fatal error paths ---

    [Fact]
    public async Task MalformedJson_SetsFatalError()
    {
        var service = BuildService();
        var raw = new RawBatch(ValidTopic, "{ not valid json }", DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }

    [Fact]
    public async Task WrongTopic_SetsFatalError()
    {
        var service = BuildService();
        var differentPatient = Guid.NewGuid();
        var raw = new RawBatch(
            $"biometrics/{differentPatient}/telemetry",
            TestPayload(),
            DateTime.UtcNow
        );

        var context = await service.ProcessAsync(raw);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.InvalidTopic, context.FatalError!.Code);
    }

    [Fact]
    public async Task UnsupportedSchemaVersion_SetsFatalError()
    {
        var service = BuildService();
        var payload = TestPayload().Replace("\"schemaVersion\": 1", "\"schemaVersion\": 99");
        var raw = new RawBatch(ValidTopic, payload, DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.UnsupportedSchemaVersion, context.FatalError!.Code);
    }

    [Fact]
    public async Task MissingPatientId_SetsFatalError()
    {
        var service = BuildService();
        var doc = JsonDocument.Parse(TestPayload());
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(doc.RootElement)!;
        dict.Remove("patientId");
        var payload = JsonSerializer.Serialize(dict);
        var raw = new RawBatch(ValidTopic, payload, DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.NotNull(context.FatalError);
    }
}
