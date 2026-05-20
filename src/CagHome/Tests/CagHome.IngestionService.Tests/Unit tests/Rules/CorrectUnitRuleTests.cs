using CagHome.IngestionService.Application.Validation.MeasurementValidation;
using CagHome.IngestionService.Domain.Enums;

namespace CagHome.IngestionService.Tests.UnitTests;

public class CorrectUnitRuleTests
{
    private readonly CorrectUnitRule _rule = new();

    [Theory]
    [InlineData(MeasurementType.HeartRate, Unit.Bpm)]
    [InlineData(MeasurementType.Spo2, Unit.Percent)]
    [InlineData(MeasurementType.BodyTemperature, Unit.C)]
    [InlineData(MeasurementType.BodyTemperature, Unit.F)]
    public async Task ValidUnit_ReturnsNull(MeasurementType type, Unit unit)
    {
        var measurement = TestDataFactory.MakeMeasurement(type, unit);

        var result = await _rule.ValidateAsync(measurement);

        Assert.Null(result);
    }

    //Invalid units

    [Theory]
    [InlineData(MeasurementType.HeartRate, Unit.Percent)]
    [InlineData(MeasurementType.HeartRate, Unit.C)]
    [InlineData(MeasurementType.HeartRate, Unit.F)]
    [InlineData(MeasurementType.Spo2, Unit.Bpm)]
    [InlineData(MeasurementType.Spo2, Unit.C)]
    [InlineData(MeasurementType.BodyTemperature, Unit.Bpm)]
    [InlineData(MeasurementType.BodyTemperature, Unit.Percent)]
    public async Task InvalidUnit_ReturnsValidationError(MeasurementType type, Unit unit)
    {
        var measurement = TestDataFactory.MakeMeasurement(type, unit);

        var result = await _rule.ValidateAsync(measurement);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.InvalidUnit, result!.Code);
    }

    [Theory]
    [InlineData(MeasurementType.HeartRate, Unit.Percent)]
    [InlineData(MeasurementType.Spo2, Unit.Bpm)]
    [InlineData(MeasurementType.BodyTemperature, Unit.Bpm)]
    public async Task InvalidUnit_ErrorMessage_ContainsMeasurementTypeAndUnit(
        MeasurementType type,
        Unit unit
    )
    {
        var measurement = TestDataFactory.MakeMeasurement(type, unit);

        var result = await _rule.ValidateAsync(measurement);

        Assert.Contains(type.ToString(), result!.Message);
        Assert.Contains(unit.ToString(), result.Message);
    }
}
