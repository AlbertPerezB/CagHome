using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Domain.Models.DataTransferObjects;

namespace CagHome.IngestionService.Application.Pipeline.Handlers;

/// <summary>
/// Maps a <see cref="BatchDto"/> from the context to a <see cref="Batch"/> domain model.
/// Sets a fatal error if the DTO is null or contains unparseable fields.
/// </summary>
public class BatchMappingHandler(ILogger<BatchMappingHandler> logger) : IngestionHandler
{
    /// <summary>
    /// Maps BatchDto from the context to a Batch domain model. If any required fields are
    /// missing or if there are parsing errors, sets a FatalError on the context and returns immediately.
    /// </summary>
    /// <param name="context">The ingestion context containing the BatchDto to map.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override Task ProcessAsync(IngestionContext context)
    {
        logger.LogDebug("Starting BatchMapping");
        var dto = context.BatchDto;

        if (
            dto is null
            || dto.CorrelationId is null
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
            CorrelationId = dto.CorrelationId.Value,
            PatientId = dto.PatientId.Value,
            SchemaVersion = dto.SchemaVersion.Value,
            AppVersion = dto.AppVersion,
            Measurements = measurements,
            ReceivedAt = context.RawBatch.ReceivedAt,
        };

        return Task.CompletedTask;
    }

    /// <summary>
    /// Maps DeviceDto to DeviceInfo.
    /// </summary>
    /// <param name="source">The source DeviceDto to map from.</param>
    /// <returns>A DeviceInfo instance with properties mapped from the source, or a new instance with
    /// null properties if the source is null.</returns>
    private static DeviceInfo MapDeviceInfo(DeviceDto? source)
    {
        return source is null ? new DeviceInfo() : new DeviceInfo(source);
    }
}
