using CagHome.IngestionService.Application.Validation.MeasurementValidation;
using CagHome.IngestionService.Domain.Enums;

namespace CagHome.IngestionService.Tests.UnitTests;

public class DeviceReportedNotInFutureRuleTest
{
    private readonly DeviceReportedNotInFutureRule _rule = new();

    [Fact]
    public async Task ValidTimestamp_NoValidationError()
    {
        var measurement = TestDataFactory.MakeMeasurement(
            deviceReported: DateTime.UtcNow.AddHours(-1)
        );

        var result = await _rule.ValidateAsync(measurement);

        Assert.Null(result);
    }

    //Invalid timestamp

    [Fact]
    public async Task InvalidTimestamp_ReturnsValidationError()
    {
        var measurement = TestDataFactory.MakeMeasurement(
            deviceReported: DateTime.UtcNow.AddHours(1)
        );

        var result = await _rule.ValidateAsync(measurement);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.DeviceReportedInFuture, result!.Code);
    }
}
