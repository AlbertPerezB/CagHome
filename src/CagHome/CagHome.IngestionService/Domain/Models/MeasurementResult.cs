using CagHome.IngestionService.Application.Validation;

namespace CagHome.IngestionService.Domain.Models
{
    public record MeasurementResult
    {
        public Measurement? Measurement { get; init; }
        public required ValidationResult ValidationResult { get; init; }
        public bool IsValid => ValidationResult.IsValid;
    }
}
