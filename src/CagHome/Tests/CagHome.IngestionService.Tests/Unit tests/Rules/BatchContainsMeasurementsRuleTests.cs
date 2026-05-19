using CagHome.IngestionService.Application.Validation.BatchValidation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Tests.UnitTests;

public class BatchContainsMeasurementsRuleTests
{
    private readonly BatchContainsMeasurementsRule _rule = new();

    [Fact]
    public async Task BatchWithMeasurements_ReturnsNull()
    {
        var batch = TestDataFactory.MakeBatch();

        var result = await _rule.ValidateAsync(batch);

        Assert.Null(result);
    }

    [Fact]
    public async Task EmptyMeasurements_ReturnsValidationError()
    {
        var batch = TestDataFactory.MakeBatch(measurements: []);

        var result = await _rule.ValidateAsync(batch);

        Assert.NotNull(result);
    }
}
