using System.Net.Mime;
using CagHome.IngestionService.Application.Pipeline.Handlers;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Domain.Models.DataTransferObjects;
using Humanizer;
using Microsoft.Extensions.Logging.Abstractions;

namespace CagHome.IngestionService.Tests.UnitTests;

public class BatchMappingHandlerTests
{
    private readonly BatchMappingHandler _handler = new BatchMappingHandler(
        new NullLogger<BatchMappingHandler>()
    );

    [Fact]
    public async Task ValidDto_MapsToBatch()
    {
        var dto = TestDataFactory.MakeBatchDto();
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

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
        var dto = TestDataFactory.MakeBatchDto();
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

        await _handler.HandleAsync(context);

        Assert.Equal(context.RawBatch.ReceivedAt, context.Batch!.ReceivedAt);
    }

    [Fact]
    public async Task ValidDto_BatchId_IsNewGuid()
    {
        var dto = TestDataFactory.MakeBatchDto();
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

        await _handler.HandleAsync(context);

        Assert.NotEqual(Guid.Empty, context.Batch!.BatchId);
    }

    [Fact]
    public async Task ValidDto_MeasurementId_PreservedWhenProvided()
    {
        var knownId = Guid.NewGuid();
        var dto = TestDataFactory.MakeBatchDto();
        dto.Measurements![0].MeasurementId = knownId;
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

        await _handler.HandleAsync(context);

        Assert.Equal(knownId, context.Batch!.Measurements[0].MeasurementId);
    }

    [Fact]
    public async Task ValidDto_MeasurementId_GeneratedWhenNull()
    {
        var dto = TestDataFactory.MakeBatchDto();
        dto.Measurements![0].MeasurementId = null;
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

        await _handler.HandleAsync(context);

        Assert.NotEqual(Guid.Empty, context.Batch!.Measurements[0].MeasurementId);
    }

    [Fact]
    public async Task ValidDto_EnumsParsedCaseInsensitive()
    {
        var dto = TestDataFactory.MakeBatchDto();
        dto.Measurements![0].Type = "heartrate";
        dto.Measurements![0].Unit = "bpm";
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.Equal(MeasurementType.HeartRate, context.Batch!.Measurements[0].MeasurementType);
        Assert.Equal(Unit.Bpm, context.Batch.Measurements[0].Unit);
    }

    [Fact]
    public async Task ValidDto_NullSource_FallsBackToDefaultDeviceInfo()
    {
        var dto = TestDataFactory.MakeBatchDto();
        dto.Measurements![0].Source = null;
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.NotNull(context.Batch!.Measurements[0].Source);
    }

    [Fact]
    public async Task ValidDto_EmptyMeasurements_MapsToEmptyList()
    {
        var dto = TestDataFactory.MakeBatchDto();
        dto.Measurements = [];
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

        await _handler.HandleAsync(context);

        Assert.Null(context.FatalError);
        Assert.Empty(context.Batch!.Measurements);
    }

    [Fact]
    public async Task NullDto_SetsFatalError()
    {
        var context = TestDataFactory.MakeContext(TestDataFactory.DefaultTopic, payload: null);

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
        var dto = TestDataFactory.MakeBatchDto();
        if (missingField == "patientId")
            dto.PatientId = null;
        if (missingField == "schemaVersion")
            dto.SchemaVersion = null;
        if (missingField == "appVersion")
            dto.AppVersion = null;
        if (missingField == "measurements")
            dto.Measurements = null;
        var context = TestDataFactory.MakeContext();

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.MissingRequiredField, context.FatalError!.Code);
    }

    [Fact]
    public async Task UnknownMeasurementType_SetsFatalError()
    {
        var dto = TestDataFactory.MakeBatchDto();
        dto.Measurements![0].Type = "BloodPressure_INVALID";
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }

    [Fact]
    public async Task UnknownUnit_SetsFatalError()
    {
        var dto = TestDataFactory.MakeBatchDto();
        dto.Measurements![0].Unit = "PARSECS";
        var context = TestDataFactory.MakeContext();
        context.BatchDto = dto;

        await _handler.HandleAsync(context);

        Assert.NotNull(context.FatalError);
        Assert.Equal(ValidationCode.ParseError, context.FatalError!.Code);
    }

    [Fact]
    public async Task FatalError_PreventsNextHandlerFromRunning()
    {
        var context = TestDataFactory.MakeContext(TestDataFactory.DefaultTopic, payload: null);
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
