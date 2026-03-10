namespace CagHome.IngestionService.Domain.Models;

public class BatchDto
{
    public int? SchemaVersion { get; set; }

    public Guid? BatchId { get; set; }

    public Guid? PatientId { get; set; }

    public string? AppVersion { get; set; }

    public DeviceDto? Device { get; set; }

    public List<MeasurementDto>? Measurements { get; set; }
}
