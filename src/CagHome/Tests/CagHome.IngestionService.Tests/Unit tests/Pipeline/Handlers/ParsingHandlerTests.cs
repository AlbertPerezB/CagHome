using System.Text.Json;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CagHome.IngestionService.Tests.Pipeline.Handlers;

public class ParseJsonHandlerTests
{
    private readonly ParseJsonHandler _handler = new ParseJsonHandler(
        new NullLogger<ParseJsonHandler>()
    );

    private static IngestionContext MakeContext(string payload) =>
        new(new RawBatch("patient/123", payload, DateTime.UtcNow));

    [Fact]
    public async Task ValidJson_ParsesJsonDocument()
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
        Assert.Null(context.BatchDto); // BatchDto should NOT be set yet

        // Verify we can read from the JsonDocument
        var schemaVersion = context.Json!.RootElement.GetProperty("schemaVersion").GetInt32();
        Assert.Equal(1, schemaVersion);
    }

    [Fact]
    public async Task MalformedJson_SetsFatalError()
    {
        var context = MakeContext("{ this is not json }");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
        Assert.Null(context.Json);
    }

    [Fact]
    public async Task EmptyPayload_SetsFatalError()
    {
        var context = MakeContext("");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
        Assert.Null(context.Json);
    }

    [Fact]
    public async Task UnterminatedString_SetsFatalError()
    {
        var context = MakeContext("""{"key": "unterminated""");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }

    [Fact]
    public async Task TrailingComma_SetsFatalError()
    {
        var context = MakeContext("""{"key": "value",}""");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }
}
