using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace CagHome.IngestionService.Tests.UnitTests;

public class TopicValidationHandlerTests
{
    private readonly TopicValidationHandler _handler = new TopicValidationHandler(
        new NullLogger<TopicValidationHandler>()
    );

    [Fact]
    public async Task MatchingTopicAndPatientId_ReturnsNull()
    {
        var context = TestDataFactory.MakeContext();
        context.Batch = TestDataFactory.MakeBatch();

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
    }

    [Theory]
    [InlineData("biometrics/not-a-guid/telemetry")]
    [InlineData("biometrics/telemetry")]
    [InlineData("patient/a1b2c3d4-0000-0000-0000-000000000000/telemetry")]
    [InlineData("biometrics/a1b2c3d4-0000-0000-0000-000000000000/measurements")]
    [InlineData("biometrics/a1b2c3d4-0000-0000-0000-000000000000")]
    public async Task MalformedTopic_SetsFatalError(string topic)
    {
        var context = TestDataFactory.MakeContext(topic);
        context.Batch = TestDataFactory.MakeBatch();

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.InvalidTopic, context.FatalError!.Code);
    }

    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n ")]
    [Theory]
    public async Task MissingTopic_SetsFatalError(string topic)
    {
        var context = TestDataFactory.MakeContext(topic: topic);

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.MissingRequiredField, context.FatalError!.Code);
    }
}
