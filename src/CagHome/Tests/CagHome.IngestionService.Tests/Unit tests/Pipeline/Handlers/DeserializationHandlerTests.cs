using System.Text.Json;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace CagHome.IngestionService.Tests.UnitTests;

public class DeserializationHandlerTests
{
    private readonly DeserializationHandler _handler = new DeserializationHandler(
        new NullLogger<DeserializationHandler>()
    );

    [Fact]
    public async Task ValidJsonDocument_DeserializesBatchDto()
    {
        var payload = TestDataFactory.ValidJsonPayload();
        var context = TestDataFactory.MakeContext(payload: payload);
        context.Json = JsonDocument.Parse(payload);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.NotNull(context.BatchDto);
        Assert.Equal(1, context.BatchDto!.SchemaVersion);
        Assert.Equal(TestDataFactory.DefaultPatientId, context.BatchDto.PatientId);
    }

    [Fact]
    public async Task ValidJson_WithMeasurements_DeserializesMeasurementDtos()
    {
        var payload = TestDataFactory.ValidJsonPayload();
        var context = TestDataFactory.MakeContext(payload: payload);
        context.Json = JsonDocument.Parse(payload);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.NotNull(context.BatchDto);
        Assert.Equal(13, context.BatchDto!.Measurements!.Count());
        Assert.Equal("HeartRate", context.BatchDto.Measurements![0].Type);
        Assert.Equal(68.0, context.BatchDto.Measurements[0].Value);
    }

    [Fact]
    public async Task CaseInsensitivePropertyNames_Deserializes()
    {
        var payload = """
            {
                "SCHEMAVERSION": 1,
                "appversion": "1.0.0",
                "PatientId": "a1b2c3d4-0000-0000-0000-000000000000",
                "CORRELATIONID": "00000000-0000-0000-0000-000000000000",
                "MEASUREMENTS": []
            }
            """;
        var context = TestDataFactory.MakeContext(payload: payload);
        context.Json = JsonDocument.Parse(payload);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.NotNull(context.BatchDto);
        Assert.Equal(1, context.BatchDto!.SchemaVersion);
    }

    [Fact]
    public async Task SkipsProcessing_WhenFatalErrorAlreadySet()
    {
        var payload = TestDataFactory.ValidJsonPayload();
        var context = TestDataFactory.MakeContext(payload: payload);
        context.Json = JsonDocument.Parse(payload);

        context.FatalError = new ValidationError(ValidationCode.ParseError, "Already failed");

        await _handler.HandleAsync(context);

        // Should not overwrite the existing error
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
        Assert.Null(context.BatchDto); // Should not deserialize
    }
}
