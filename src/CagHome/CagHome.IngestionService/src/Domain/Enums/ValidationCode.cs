namespace CagHome.IngestionService.Domain.Enums
{
    public enum ValidationCode
    {
        //structural
        InvalidSchema,
        UnsupportedSchemaVersion,
        ParseError,

        //batch
        PatientInactive,
        MissingRequiredField,
        InvalidUnit,
        DeviceReportedInFuture,
        DeviceReportedTooOld,
        TopicPatientIdMismatch,
    }
}
