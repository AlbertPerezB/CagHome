using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using Xunit;

namespace CagHome.IngestionService.Tests.Pipeline.Handlers;

public class TopicValidationHandlerTests
{
    private readonly TopicValidationHandler _handler = new();

    private static IngestionContext MakeContext(Guid patientId, string topic)
    {
        var payload = $$"""
            {
                "schemaVersion": 1,
                "appVersion": "1.0.0",
                "patientId": "{{patientId}}",
                "measurements": []
            }
            """;
        var raw = new RawBatch(topic, payload, DateTime.UtcNow);
        return new IngestionContext(raw)
        {
            Batch = new Batch
            {
                BatchId = Guid.NewGuid(),
                PatientId = patientId,
                SchemaVersion = 1,
                AppVersion = new Version(1, 0, 0),
                Measurements = [],
                ReceivedAt = DateTime.UtcNow,
            },
        };
    }

    [Fact]
    public async Task MatchingTopicAndPatientId_ReturnsNull()
    {
        var patientId = Guid.NewGuid();
        var context = MakeContext(patientId, $"biometrics/{patientId}/telemetry");

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
        var context = MakeContext(Guid.NewGuid(), topic);

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
        var context = MakeContext(Guid.NewGuid(), topic);

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.MissingRequiredField, context.FatalError!.Code);
    }
}
