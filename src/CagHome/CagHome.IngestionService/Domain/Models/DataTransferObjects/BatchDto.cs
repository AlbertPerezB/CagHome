namespace CagHome.IngestionService.Domain.Models.DataTransferObjects;

public class BatchDto
{
    public int? SchemaVersion { get; set; }

    public Version? AppVersion { get; set; }

    public Guid? CorrelationId { get; set; }

    public Guid? PatientId { get; set; }

    public List<MeasurementDto>? Measurements { get; set; }
}
