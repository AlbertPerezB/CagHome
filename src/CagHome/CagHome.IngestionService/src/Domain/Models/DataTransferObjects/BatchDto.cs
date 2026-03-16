namespace CagHome.IngestionService.Domain.Models;

public class BatchDto
{
    public int? SchemaVersion { get; set; }

    public Version? AppVersion { get; set; }

    public Guid? PatientId { get; set; }

    public List<MeasurementDto>? Measurements { get; set; }
}
