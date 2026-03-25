using CagHome.IngestionService.Application.Validation.BatchValidation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using Xunit;

namespace CagHome.IngestionService.Tests.Validation.BatchValidation;

public class BatchContainsMeasurementsRuleTests
{
    private readonly BatchContainsMeasurementsRule _rule = new();

    private static Batch MakeBatch(List<Measurement> measurements) =>
        new()
        {
            BatchId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            SchemaVersion = 1,
            AppVersion = new Version(1, 0, 0),
            Measurements = measurements,
            ReceivedAt = DateTime.UtcNow,
        };

    private static Measurement MakeMeasurement() =>
        new()
        {
            MeasurementId = Guid.NewGuid(),
            MeasurementType = MeasurementType.HeartRate,
            Value = 72f,
            Unit = Unit.Bpm,
            DeviceReported = DateTime.UtcNow,
            Source = new DeviceInfo(),
        };

    [Fact]
    public async Task BatchWithMeasurements_ReturnsNull()
    {
        var batch = MakeBatch([MakeMeasurement()]);

        var result = await _rule.ValidateAsync(batch);

        Assert.Null(result);
    }

    [Fact]
    public async Task EmptyMeasurements_ReturnsValidationError()
    {
        var batch = MakeBatch([]);

        var result = await _rule.ValidateAsync(batch);

        Assert.NotNull(result);
    }
}
