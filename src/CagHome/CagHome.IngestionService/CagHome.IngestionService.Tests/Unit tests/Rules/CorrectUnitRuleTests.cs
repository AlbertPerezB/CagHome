using CagHome.IngestionService.Application.Validation.MeasurementValidation;
using CagHome.IngestionService.Domain.Models;
using Xunit;

public class CorrectUnitRuleTests
{
    [Fact]
    public void InvalidUnit_Should_Return_Error()
    {
        var rule = new CorrectUnitRule();

        var measurement = new Measurement { Type = "heartRate", Unit = "kg" };

        var result = rule.Validate(measurement);

        Assert.NotNull(result);
    }
}
