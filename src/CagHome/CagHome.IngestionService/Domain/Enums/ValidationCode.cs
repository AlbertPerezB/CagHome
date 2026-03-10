namespace CagHome.IngestionService.Domain.Enums
{
    public enum ValidationCode
    {
        PatientInactive,
        InvalidSchema,
        MissingRequiredField,
        InvalidFieldValue,
        DeviceReportedInFuture,
        DeviceReportedTooOld,
        TopicPatientIdMismatch,
    }
}
