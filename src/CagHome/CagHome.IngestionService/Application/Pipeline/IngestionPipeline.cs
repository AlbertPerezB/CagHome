using CagHome.IngestionService.Application.Validation;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace CagHome.IngestionService.Application.Pipeline
{
    public class IngestionPipeline
    {
        private BatchValidator _batchValidator;
        private MeasurementValidator _measurementValidator;
        private RabbitMqPublisher _publisher;
        private BatchParser _parser;
        private ILogger<IngestionPipeline> _logger;

        public IngestionPipeline(
            BatchValidator batchValidator,
            MeasurementValidator measurementValidator,
            RabbitMqPublisher publisher,
            BatchParser parser,
            ILogger<IngestionPipeline> logger
        )
        {
            _batchValidator = batchValidator;
            _measurementValidator = measurementValidator;
            _publisher = publisher;
            _parser = parser;
            _logger = logger;
        }

        public async Task ProcessAsync(RawBatch rawBatch, CancellationToken cancellationToken)
        {
            //parse batch
            _logger.LogDebug($"Parsing raw payload for topic {rawBatch.Topic}");
            var batch = _parser.Parse(rawBatch.RawPayload);
            if (batch == null)
            {
                _logger.LogWarning($"Failed to parse raw payload for topic {rawBatch.Topic}");
                return;
            }

            //batch validation
            _logger.LogDebug($"Starting batch validation for topic {rawBatch.Topic}");
            var batchValidationResult = await _batchValidator.ValidateAsync(
                batch,
                cancellationToken
            );
            if (!batchValidationResult.IsValid)
            {
                _logger.LogWarning($"Batch validation failed.");
                return;
            }

            //measurement validation
            _logger.LogDebug($"Starting measurement validation for topic {rawBatch.Topic}");
            foreach (var measurement in batch.Measurements)
            {
                var measurementValidationResult = await _measurementValidator.ValidateAsync(
                    measurement,
                    cancellationToken
                );
                if (!measurementValidationResult.IsValid)
                {
                    _logger.LogWarning(
                        $"Some measurement validations failed for topic {rawBatch.Topic}"
                    );
                }
            }
        }
    }
}
