using System.Text.Json;
using CagHome.Contracts;
using CagHome.Contracts.Enums;
using CagHome.IngestionService.Application;
using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Application.Validation.BatchValidation;
using CagHome.IngestionService.Application.Validation.MeasurementValidation;
using CagHome.IngestionService.Application.Validation.StructuralValidation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Cache;
using CagHome.IngestionService.Infrastructure.Schemas;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Wolverine;
using Xunit;

namespace CagHome.IngestionService.Tests.Integration;

public class IngestionServiceIntegrationTests
{
    private static readonly Guid _patientId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000000");
    private static readonly string _validTopic = $"biometrics/{_patientId}/telemetry";

    private static string ValidTestPayload() => File.ReadAllText("TestData/test_batch_valid.json");

    private static string InvalidMeasurementsPayload() =>
        File.ReadAllText("TestData/test_batch_invalid_measurements.json");

    private static IIngestionService BuildService(
        PatientStatus? patientStatusOverride = null,
        PublishBatchHandler? publishOverride = null,
        ErrorHandler? errorOverride = null
    )
    {
        var registry = new JsonSchemaRegistry();
        var messageBus = Substitute.For<IMessageBus>();

        var parseJson = new ParseJsonHandler(new NullLogger<ParseJsonHandler>());
        var deserialization = new DeserializationHandler(new NullLogger<DeserializationHandler>());
        var structuralRules = new List<IValidationRule<JsonDocument>>
        {
            new SchemaValidationRule(registry),
        };
        var structural = new StructuralValidationHandler(
            new StructuralValidator(structuralRules),
            new NullLogger<StructuralValidationHandler>()
        );
        var batchMapping = new BatchMappingHandler(new NullLogger<BatchMappingHandler>());
        var topicValidation = new TopicValidationHandler(new NullLogger<TopicValidationHandler>());

        var patientCache = CreateCacheWithStatus(patientStatusOverride ?? PatientStatus.Active);
        var batchRules = new List<IBatchValidationRule> { new PatientActiveRule(patientCache) };
        var batchValidator = new BatchValidator(batchRules);
        var batchValidation = new BatchValidationHandler(
            batchValidator,
            new NullLogger<BatchValidationHandler>()
        );
        var measurementRules = new List<IValidationRule<Measurement>>
        {
            new CorrectUnitRule(),
            new DeviceReportedNotInFutureRule(),
        };
        var measurementValidator = new MeasurementValidator(measurementRules);
        var measurementValidation = new MeasurementValidationHandler(
            measurementValidator,
            new NullLogger<MeasurementValidationHandler>()
        );

        var publish =
            publishOverride
            ?? new PublishBatchHandler(messageBus, new NullLogger<PublishBatchHandler>());
        var errors = errorOverride ?? new ErrorHandler(new NullLogger<ErrorHandler>());

        var pipeline = IngestionPipelineBuilder.Build(
            structural,
            parseJson,
            deserialization,
            batchMapping,
            topicValidation,
            batchValidation,
            measurementValidation,
            publish,
            errors
        );

        return new Application.IngestionService(pipeline);
    }

    private static IPatientRegistryCache CreateCacheWithStatus(PatientStatus cacheStatus)
    {
        var cache = Substitute.For<IPatientRegistryCache>();
        cache.GetPatientStatus(_patientId).Returns(cacheStatus);
        return cache;
    }

    [Fact]
    public async Task ValidBatch_PipelineCompletesWithNoFatalError()
    {
        var service = BuildService();
        var raw = new RawBatch(_validTopic, ValidTestPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.Null(context.FatalError);
    }

    [Fact]
    public async Task ValidBatch_BatchIsMapped()
    {
        var service = BuildService();
        var raw = new RawBatch(_validTopic, ValidTestPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.NotNull(context.Batch);
        Assert.Equal(_patientId, context.Batch!.PatientId);
        Assert.Equal(1, context.Batch.SchemaVersion);
    }

    [Fact]
    public async Task ValidBatch_AllMeasurementsMapped()
    {
        var service = BuildService();
        var raw = new RawBatch(_validTopic, ValidTestPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.Equal(13, context.Batch!.Measurements.Count);
    }

    [Fact]
    public async Task ValidBatch_NoMeasurementValidationErrors()
    {
        var service = BuildService();
        var raw = new RawBatch(_validTopic, ValidTestPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.All(context.Batch!.Measurements, m => Assert.Empty(m.ValidationErrors));
    }

    [Fact]
    public async Task ValidBatch_ShouldPublishBatchReceivedMessage()
    {
        var messageBus = Substitute.For<IMessageBus>();
        var publishHandler = new PublishBatchHandler(
            messageBus,
            new NullLogger<PublishBatchHandler>()
        );
        var service = BuildService(publishOverride: publishHandler);
        var raw = new RawBatch(_validTopic, ValidTestPayload(), DateTime.UtcNow);

        await service.ProcessAsync(raw);

        await messageBus
            .Received(1)
            .PublishAsync(
                Arg.Is<BatchReceived>(br =>
                    br.PatientId == _patientId && br.Measurements.Count == 13
                )
            );
    }

    [Fact]
    public async Task FatalError_ShouldNotPublishBatchReceived()
    {
        var messageBus = Substitute.For<IMessageBus>();
        var publishHandler = new PublishBatchHandler(
            messageBus,
            new NullLogger<PublishBatchHandler>()
        );
        var service = BuildService(publishOverride: publishHandler);
        var raw = new RawBatch(_validTopic, "{ not valid json }", DateTime.UtcNow);

        await service.ProcessAsync(raw);

        await messageBus.DidNotReceive().PublishAsync(Arg.Any<BatchReceived>());
    }

    [Fact]
    public async Task MixedBatch_PipelineCompletesWithNoFatalError()
    {
        // Non-fatal measurement errors should not cause a fatal error.
        // The batch should still flow through the entire pipeline.
        var service = BuildService();
        var raw = new RawBatch(_validTopic, InvalidMeasurementsPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.Null(context.FatalError);
    }

    [Fact]
    public async Task MixedBatch_AllMeasurementsPreserved()
    {
        // Even measurements with errors should remain in the batch —
        // downstream services decide how to handle annotated objects.
        var service = BuildService();
        var raw = new RawBatch(_validTopic, InvalidMeasurementsPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        Assert.Equal(13, context.Batch!.Measurements.Count);
    }

    [Fact]
    public async Task MixedBatch_InvalidMeasurementsAnnotatedWithErrors()
    {
        var service = BuildService();
        var raw = new RawBatch(_validTopic, InvalidMeasurementsPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        var withErrors = context.Batch!.Measurements.Where(m => m.ValidationErrors.Any()).ToList();
        Assert.Equal(2, withErrors.Count);
    }

    [Fact]
    public async Task MixedBatch_ValidMeasurementsHaveNoErrors()
    {
        var service = BuildService();
        var raw = new RawBatch(_validTopic, InvalidMeasurementsPayload(), DateTime.UtcNow);

        var context = await service.ProcessAsync(raw);

        var clean = context.Batch!.Measurements.Where(m => !m.ValidationErrors.Any()).ToList();
        Assert.Equal(11, clean.Count);
    }

    [Fact]
    public async Task MixedBatch_ShouldStillPublishBatchReceived()
    {
        // UC4 Path B: the batch is forwarded including error annotations.
        var messageBus = Substitute.For<IMessageBus>();
        var publishHandler = new PublishBatchHandler(
            messageBus,
            new NullLogger<PublishBatchHandler>()
        );
        var service = BuildService(publishOverride: publishHandler);
        var raw = new RawBatch(_validTopic, InvalidMeasurementsPayload(), DateTime.UtcNow);

        await service.ProcessAsync(raw);

        await messageBus
            .Received(1)
            .PublishAsync(
                Arg.Is<BatchReceived>(br =>
                    br.PatientId == _patientId
                    && br.Measurements.Count == 13
                    && br.Measurements.Count(m => m.ValidationErrors.Any()) == 2
                )
            );
    }
}
