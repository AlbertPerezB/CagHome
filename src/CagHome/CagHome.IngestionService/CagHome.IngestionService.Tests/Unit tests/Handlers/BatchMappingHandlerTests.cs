using CagHome.IngestionService.Application.Pipeline;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using Xunit;

namespace CagHome.IngestionService.Tests.Pipeline.Handlers;

public class BatchMappingHandlerTests
{
    private readonly BatchMappingHandler _handler = new();

    private static IngestionContext MakeContext(BatchDto? dto = null)
    {
        var raw = new RawBatch(
            "patient/123",
            "{}",
            new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc)
        );
        var context = new IngestionContext(raw) { BatchDto = dto };
        return context;
    }

    private static MeasurementDto ValidMeasurementDto() =>
        new()
        {
            MeasurementId = Guid.NewGuid(),
            Type = "HeartRate",
            Value = 72.0,
            Unit = "BPM",
            DeviceReported = DateTime.UtcNow,
            Source = new DeviceDto
            {
                DeviceManufacturer = "Apple",
                DeviceModel = "Apple Watch Series 9",
                Platform = "iOS",
            },
        };

    private static BatchDto ValidBatchDto() =>
        new()
        {
            SchemaVersion = 1,
            AppVersion = new Version(1, 0, 0),
            PatientId = Guid.NewGuid(),
            Measurements = [ValidMeasurementDto()],
        };

    [Fact]
    public async Task ValidDto_MapsToBatch()
    {
        var dto = ValidBatchDto();
        var context = MakeContext(dto);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.NotNull(context.Batch);
        Assert.Equal(dto.PatientId!.Value, context.Batch!.PatientId);
        Assert.Equal(dto.SchemaVersion!.Value, context.Batch.SchemaVersion);
        Assert.Equal(dto.AppVersion, context.Batch.AppVersion);
    }

    [Fact]
    public async Task ValidDto_ReceivedAt_TakenFromRawBatch()
    {
        var context = MakeContext(ValidBatchDto());

        await _handler.HandleAsync(context);

        Assert.Equal(context.RawBatch.ReceivedAt, context.Batch!.ReceivedAt);
    }

    [Fact]
    public async Task ValidDto_BatchId_IsNewGuid()
    {
        var context = MakeContext(ValidBatchDto());

        await _handler.HandleAsync(context);

        Assert.NotEqual(Guid.Empty, context.Batch!.BatchId);
    }

    [Fact]
    public async Task ValidDto_MeasurementId_PreservedWhenProvided()
    {
        var knownId = Guid.NewGuid();
        var dto = ValidBatchDto();
        dto.Measurements![0].MeasurementId = knownId;
        var context = MakeContext(dto);

        await _handler.HandleAsync(context);

        Assert.Equal(knownId, context.Batch!.Measurements[0].MeasurementId);
    }

    [Fact]
    public async Task ValidDto_MeasurementId_GeneratedWhenNull()
    {
        var dto = ValidBatchDto();
        dto.Measurements![0].MeasurementId = null;
        var context = MakeContext(dto);

        await _handler.HandleAsync(context);

        Assert.NotEqual(Guid.Empty, context.Batch!.Measurements[0].MeasurementId);
    }

    [Fact]
    public async Task ValidDto_EnumsParsedCaseInsensitive()
    {
        var dto = ValidBatchDto();
        dto.Measurements![0].Type = "heartrate";
        dto.Measurements![0].Unit = "bpm";
        var context = MakeContext(dto);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.Equal(MeasurementType.HeartRate, context.Batch!.Measurements[0].MeasurementType);
        Assert.Equal(Unit.Bpm, context.Batch.Measurements[0].Unit);
    }

    [Fact]
    public async Task ValidDto_NullSource_FallsBackToDefaultDeviceInfo()
    {
        var dto = ValidBatchDto();
        dto.Measurements![0].Source = null;
        var context = MakeContext(dto);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.NotNull(context.Batch!.Measurements[0].Source);
    }

    [Fact]
    public async Task ValidDto_EmptyMeasurements_MapsToEmptyList()
    {
        var dto = ValidBatchDto();
        dto.Measurements = [];
        var context = MakeContext(dto);

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.Empty(context.Batch!.Measurements);
    }

    // --- Fatal error: missing required fields ---

    [Fact]
    public async Task NullDto_SetsFatalError()
    {
        var context = MakeContext(null);

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.MissingRequiredField, context.FatalError!.Code);
    }

    [Theory]
    [InlineData("patientId")]
    [InlineData("schemaVersion")]
    [InlineData("appVersion")]
    [InlineData("measurements")]
    public async Task MissingRequiredField_SetsFatalError(string missingField)
    {
        var dto = ValidBatchDto();
        if (missingField == "patientId")
            dto.PatientId = null;
        if (missingField == "schemaVersion")
            dto.SchemaVersion = null;
        if (missingField == "appVersion")
            dto.AppVersion = null;
        if (missingField == "measurements")
            dto.Measurements = null;
        var context = MakeContext(dto);

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.MissingRequiredField, context.FatalError!.Code);
    }

    // --- Fatal error: bad enums ---

    [Fact]
    public async Task UnknownMeasurementType_SetsFatalError()
    {
        var dto = ValidBatchDto();
        dto.Measurements![0].Type = "BloodPressure_INVALID";
        var context = MakeContext(dto);

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }

    [Fact]
    public async Task UnknownUnit_SetsFatalError()
    {
        var dto = ValidBatchDto();
        dto.Measurements![0].Unit = "PARSECS";
        var context = MakeContext(dto);

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }

    [Fact]
    public async Task FatalError_PreventsNextHandlerFromRunning()
    {
        var context = MakeContext(null);
        var nextCalled = false;
        var next = new DelegateHandler(() =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        _handler.SetNext(next);

        await _handler.HandleAsync(context);

        Assert.False(nextCalled);
    }
}
