using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text.Json;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation
{
    public class SchemaVersionFoundRule : IValidationRule<RawBatch>
    {
        public bool IsFatal => true;

        public Task<ValidationError?> ValidateAsync(RawBatch rawBatch)
        {
            var version = GetVersionFromJson(rawBatch.RawPayload);
            if (version == null)
            {
                return Task.FromResult<ValidationError?>(
                    new ValidationError(
                        $"No field corresponding to schema version",
                        ValidationCode.UnsupportedSchemaVersion
                    )
                );
            }

            return Task.FromResult<ValidationError?>(null);
        }

        private int? GetVersionFromJson(JsonElement json)
        {
            try
            {
                json.TryGetProperty("schemaVersion", out var version);
                return version.GetInt32();
            }
            catch
            {
                return null;
            }
        }
    }
}
