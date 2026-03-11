namespace CagHome.IngestionService.Domain.Models;

public class MeasurementDto
{
    public Guid? MeasurementId { get; set; }

    public string? Type { get; set; }

    public double? Value { get; set; }

    public string? Unit { get; set; }

    public DateTime? DeviceReported { get; set; }

    public DeviceDto? Source { get; set; }
}
