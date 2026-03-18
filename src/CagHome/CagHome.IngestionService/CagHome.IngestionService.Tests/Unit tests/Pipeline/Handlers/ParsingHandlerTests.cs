using System.Text.Json;
using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using Xunit;

namespace CagHome.IngestionService.Tests.Pipeline.Handlers;

public class ParseJsonHandlerTests
{
    private readonly ParseJsonHandler _handler = new();

    private static IngestionContext MakeContext(string payload) =>
        new(new RawBatch("patient/123", payload, DateTime.UtcNow));

    [Fact]
    public async Task ValidJson_ParsesJsonDocumentAndBatchDto()
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
        Assert.NotNull(context.Json);
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
        Assert.Single(context.BatchDto!.Measurements!);
        Assert.Equal("HeartRate", context.BatchDto.Measurements![0].Type);
    }

    [Fact]
    public async Task MalformedJson_SetsFatalError()
    {
        var context = MakeContext("{ this is not json }");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }

    [Fact]
    public async Task EmptyPayload_SetsFatalError()
    {
        var context = MakeContext("");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }

    [Fact]
    public async Task ValidJson_DoesNotCallNext_WhenFatalErrorSet()
    {
        var context = MakeContext("{ not valid }");
        var nextCalled = false;
        var next = new DelegateHandler(() =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        _handler.SetNext(next);

        await _handler.HandleAsync(context);

        Assert.False(nextCalled);
    }
}
