using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Models;
using Xunit;

public class ParsingHandlerTests
{
    [Fact]
    public async Task ValidJson_Should_Create_Dto()
    {
        var handler = new ParsingHandler();

        var payload = """
            {
              "schemaVersion": "1.0",
              "patientId": "123",
              "measurements": []
            }
            """;

        var rawBatch = new RawBatch("topic", payload, DateTime.UtcNow);
        var context = new IngestionContext(rawBatch);

        await handler.HandleAsync(context);

        Assert.NotNull(context.BatchDto);
        Assert.Null(context.FatalError);
    }

    [Fact]
    public async Task InvalidJson_Should_Set_ParseError()
    {
        var handler = new ParsingHandler();

        var rawBatch = new RawBatch("topic", "{invalid json", DateTime.UtcNow);
        var context = new IngestionContext(rawBatch);

        await handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Null(context.BatchDto);
    }

    [Fact]
    public async Task Empty_Payload_Should_Set_ParseError()
    {
        var handler = new ParsingHandler();

        var rawBatch = new RawBatch("topic", "", DateTime.UtcNow);
        var context = new IngestionContext(rawBatch);

        await handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Null(context.BatchDto);
    }
}
