using CagHome.IngestionService.Application.Pipeline.Handlers;

namespace CagHome.IngestionService.Application.Pipeline;

public static class IngestionPipelineBuilder
{
    public static IIngestionHandler Build(
        StructuralValidationHandler structural,
        ParseJsonHandler jsonParser,
        BatchMappingHandler batchMapping,
        TopicValidationHandler topicValidation,
        BatchValidationHandler batch,
        MeasurementValidationHandler measurement,
        IIngestionHandler publish, //TODO: implement for real
        IIngestionHandler errors //TODO: implement for real
    )
    {
        jsonParser
            .SetNext(structural)
            .SetNext(batchMapping)
            .SetNext(topicValidation)
            .SetNext(batch)
            .SetNext(measurement)
            .SetNext(publish)
            .SetNext(errors);

        return jsonParser;
    }
}
