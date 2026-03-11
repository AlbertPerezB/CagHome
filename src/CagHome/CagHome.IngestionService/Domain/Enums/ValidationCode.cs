namespace CagHome.IngestionService.Domain.Enums
{
    public enum ValidationCode
    {
        InvalidSchema,
        UnsupportedSchemaVersion,

        PatientInactive,
        MissingRequiredField,
        InvalidUnit,
        DeviceReportedInFuture,
        DeviceReportedTooOld,
        TopicPatientIdMismatch,
    }
}
