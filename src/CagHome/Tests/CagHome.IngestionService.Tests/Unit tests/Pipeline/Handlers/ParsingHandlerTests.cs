using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace CagHome.IngestionService.Tests.UnitTests;

public class ParseJsonHandlerTests
{
    private readonly ParseJsonHandler _handler = new ParseJsonHandler(
        new NullLogger<ParseJsonHandler>()
    );

    [Fact]
    public async Task ValidJson_ParsesJsonDocument()
    {
        var payload = TestDataFactory.ValidJsonPayload();
        var context = TestDataFactory.MakeContext(payload: payload);

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
        var context = TestDataFactory.MakeContext(payload: "{ this is not json }");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
        Assert.Null(context.Json);
    }

    [Fact]
    public async Task EmptyPayload_SetsFatalError()
    {
        var context = TestDataFactory.MakeContext(payload: "");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
        Assert.Null(context.Json);
    }

    [Fact]
    public async Task UnterminatedString_SetsFatalError()
    {
        var context = TestDataFactory.MakeContext(payload: """{"key": "unterminated""");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }

    [Fact]
    public async Task TrailingComma_SetsFatalError()
    {
        var context = TestDataFactory.MakeContext(payload: """{"key": "value",}""");

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }
}
