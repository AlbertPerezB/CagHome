using System.Text.Json;
using CagHome.IngestionService.Application.Validation;

namespace CagHome.IngestionService.Domain.Models
{
    public record RawBatch(string Topic, string Payload, DateTime ReceivedAt);
}
