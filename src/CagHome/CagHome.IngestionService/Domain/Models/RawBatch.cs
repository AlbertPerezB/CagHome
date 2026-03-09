using CagHome.IngestionService.Application.Validation;

namespace CagHome.IngestionService.Domain.Models
{
    public record RawBatch(string Topic, string RawPayload, DateTime ReceivedAt);
}
