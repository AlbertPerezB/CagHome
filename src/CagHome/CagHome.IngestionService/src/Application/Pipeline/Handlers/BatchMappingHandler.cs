using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

public class BatchMappingHandler : IngestionHandler
{
    public BatchMappingHandler(ILoggerFactory loggerFactory)
        : base(loggerFactory) { }

    protected override Task ProcessAsync(IngestionContext context)
    {
        _logger.LogDebug("Starting BatchMapping");
        var dto = context.BatchDto;

        if (
            dto is null
            || dto.PatientId is null
            || dto.SchemaVersion is null
            || dto.AppVersion is null
            || dto.Measurements is null
        )
        {
            context.FatalError = new ValidationError(
                ValidationCode.MissingRequiredField,
                "Mapping failed: Required fields missing on BatchDto."
            );
            return Task.CompletedTask;
        }

        var measurements = new List<Measurement>();

        foreach (var m in dto.Measurements)
        {
            if (!Enum.TryParse<MeasurementType>(m.Type, ignoreCase: true, out var measurementType))
            {
                context.FatalError = new ValidationError(
                    ValidationCode.ParseError,
                    $"Unknown MeasurementType: '{m.Type}'"
                );
                return Task.CompletedTask;
            }

            if (!Enum.TryParse<Unit>(m.Unit, ignoreCase: true, out var unit))
            {
                context.FatalError = new ValidationError(
                    ValidationCode.ParseError,
                    $"Unknown Unit: '{m.Unit}'"
                );
                return Task.CompletedTask;
            }

            measurements.Add(
                new Measurement
                {
                    MeasurementId = m.MeasurementId ?? Guid.NewGuid(),
                    MeasurementType = measurementType,
                    Value = m.Value ?? 0,
                    Unit = unit,
                    DeviceReported = m.DeviceReported ?? DateTime.MinValue,
                    Source = MapDeviceInfo(m.Source),
                }
            );
        }

        context.Batch = new Batch
        {
            BatchId = Guid.NewGuid(),
            PatientId = dto.PatientId.Value,
            SchemaVersion = dto.SchemaVersion.Value,
            AppVersion = dto.AppVersion,
            Measurements = measurements,
            ReceivedAt = context.RawBatch.ReceivedAt,
        };

        return Task.CompletedTask;
    }

    private static DeviceInfo MapDeviceInfo(DeviceDto? source)
    {
        return source is null ? new DeviceInfo() : new DeviceInfo(source);
    }
}
