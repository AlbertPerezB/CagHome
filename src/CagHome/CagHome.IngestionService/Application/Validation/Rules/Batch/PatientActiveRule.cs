using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using StackExchange.Redis;

namespace CagHome.IngestionService.Application.Validation
{
    public class PatientActiveRule : IValidationRule<Measurement> { }
}
