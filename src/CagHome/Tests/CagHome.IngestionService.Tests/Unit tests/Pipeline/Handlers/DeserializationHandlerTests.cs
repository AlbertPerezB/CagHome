using System.Text.Json;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CagHome.IngestionService.Tests.Pipeline.Handlers;

public class DeserializationHandlerTests
{
    private readonly DeserializationHandler _handler = new DeserializationHandler(
        new NullLogger<DeserializationHandler>()
    );

    private static IngestionContext MakeContext(string payload)
    {
        var context = new IngestionContext(new RawBatch("patient/123", payload, DateTime.UtcNow))
        {
            Json = JsonDocument.Parse(payload),
        };
        return context;
    }

    [Fact]
    public async Task ValidJsonDocument_DeserializesBatchDto()
    {
        var payload = """
            {
                "schemaVersion": 1,
                "appVersion": "1.0.0",
                "patientId": "a1b2c3d4-0000-0000-0000-000000000000",
                "measurements": []
            }
            """;
        var context = MakeContext(payload);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.NotNull(context.BatchDto);
        Assert.Equal(1, context.BatchDto!.SchemaVersion);
        Assert.Equal(
            Guid.Parse("a1b2c3d4-0000-0000-0000-000000000000"),
            context.BatchDto.PatientId
        );
    }

    [Fact]
    public async Task ValidJson_WithMeasurements_DeserializesMeasurementDtos()
    {
        var payload = """
            {
                "schemaVersion": 1,
                "appVersion": "1.0.0",
                "patientId": "a1b2c3d4-0000-0000-0000-000000000000",
                "measurements": [
                    {
                        "measurementId": "aaaaaaaa-0000-0000-0000-000000000000",
                        "type": "HeartRate",
                        "value": 72.0,
                        "unit": "BPM",
                        "deviceReported": "2024-01-01T10:00:00Z",
                        "source": { "id": "dev-1", "name": "Band", "model": "X1" }
                    }
                ]
            }
            """;
        var context = MakeContext(payload);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.NotNull(context.BatchDto);
        Assert.Single(context.BatchDto!.Measurements!);
        Assert.Equal("HeartRate", context.BatchDto.Measurements![0].Type);
        Assert.Equal(72.0, context.BatchDto.Measurements[0].Value);
    }

    [Fact]
    public async Task CaseInsensitivePropertyNames_Deserializes()
    {
        var payload = """
            {
                "SCHEMAVERSION": 1,
                "appversion": "1.0.0",
                "PatientId": "a1b2c3d4-0000-0000-0000-000000000000",
                "MEASUREMENTS": []
            }
            """;
        var context = MakeContext(payload);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.NotNull(context.BatchDto);
        Assert.Equal(1, context.BatchDto!.SchemaVersion);
    }

    [Fact]
    public async Task SkipsProcessing_WhenFatalErrorAlreadySet()
    {
        var payload =
            """{"schemaVersion": 1, "patientId": "a1b2c3d4-0000-0000-0000-000000000000"}""";
        var context = MakeContext(payload);
        context.FatalError = new ValidationError(ValidationCode.ParseError, "Already failed");

        await _handler.HandleAsync(context);

        // Should not overwrite the existing error
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
        Assert.Null(context.BatchDto); // Should not deserialize
    }
}
