using CagHome.IngestionService.Application.Validation.MeasurementValidation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using Xunit;

public class DeviceReportedNotInFutureRuleTest
{
    private readonly DeviceReportedNotInFutureRule _rule = new();

    private static Measurement MakeMeasurement(DateTime deviceReported) =>
        new()
        {
            MeasurementId = Guid.NewGuid(),
            MeasurementType = MeasurementType.BodyTemperature,
            Value = 38,
            Unit = Unit.C,
            DeviceReported = deviceReported,
            Source = new DeviceInfo(),
        };

    [Fact]
    public async Task ValidTimestamp_NoValidationError()
    {
        var measurement = MakeMeasurement(DateTime.UtcNow.AddHours(-1));

        var result = await _rule.ValidateAsync(measurement);

        Assert.Null(result);
    }

    //Invalid timestamp

    [Fact]
    public async Task InvalidTimestamp_ReturnsValidationError()
    {
        var measurement = MakeMeasurement(DateTime.UtcNow.AddHours(1));

        var result = await _rule.ValidateAsync(measurement);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.DeviceReportedInFuture, result!.Code);
    }
}
