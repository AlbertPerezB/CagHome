using CagHome.IngestionService.Application.Pipeline.Handlers;

namespace CagHome.IngestionService.Application.Pipeline;

public static class IngestionPipelineBuilder
{
    public static IIngestionHandler Build(
        StructuralValidationHandler structural,
        ParsingHandler parsing,
        BatchValidationHandler batch,
        MeasurementValidationHandler measurement,
        PublishBatchHandler publish,
        ErrorPublishingHandler errors
    )
    {
        structural
            .SetNext(parsing)
            .SetNext(batch)
            .SetNext(measurement)
            .SetNext(publish)
            .SetNext(errors);

        return structural;
    }
}
