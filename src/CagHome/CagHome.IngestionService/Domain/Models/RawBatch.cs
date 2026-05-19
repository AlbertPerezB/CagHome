namespace CagHome.IngestionService.Domain.Models
{
    public record RawBatch(string Topic, string Payload, DateTime ReceivedAt);
}
