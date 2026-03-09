using CagHome.IngestionService.Application.Validation;

namespace CagHome.IngestionService.Domain.Models
{
    public record EnrichedBatch
    {
        public Guid BatchId { get; init; }
        public DateTime ReceivedAt { get; init; }
        public required ValidationResult BatchValidation { get; init; }
        public required IReadOnlyList<MeasurementResult> MeasurementResults { get; init; }
    }
}
