namespace CagHome.IngestionService.Domain.Enums
{
    public enum ValidationCode
    {
        InvalidSchema,
        UnsupportedSchemaVersion,
        ParseError,
        PatientInactive,
        MissingRequiredField,
        InvalidUnit,
        DeviceReportedInFuture,
        DeviceReportedTooOld,
        TopicPatientIdMismatch,
    }
}
