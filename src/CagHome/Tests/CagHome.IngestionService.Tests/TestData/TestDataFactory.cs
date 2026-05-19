using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Domain.Models.DataTransferObjects;

public static class TestDataFactory
{
    public static readonly Guid DefaultPatientId = Guid.Parse(
        "a1b2c3d4-0000-0000-0000-000000000000"
    );
    public static readonly Guid DefaultCorrelationId = Guid.Parse(
        "00000000-0000-0000-0000-000000000000"
    );
    public static readonly string DefaultTopic = $"biometrics/{DefaultPatientId}/telemetry";

    public static string ValidJsonPayload() => File.ReadAllText("TestData/test_batch_valid.json");

    public static string InvalidMeasurementsPayload() =>
        File.ReadAllText("TestData/test_batch_invalid_measurements.json");

    public static string BatchJson(
        int schemaVersion = 1,
        string appVersion = "1.0.0",
        Guid? patientId = null,
        Guid? correlationId = null,
        params string[] measurements
    ) =>
        $$"""
            {
                "schemaVersion": {{schemaVersion}},
                "appVersion": "{{appVersion}}",
                "patientId": "{{patientId ?? DefaultPatientId}}",
                "correlationId": "{{correlationId ?? DefaultCorrelationId}}",
                "measurements": [{{string.Join(",", measurements)}}]
            }
            """;

    public static string MeasurementJson(
        string type = "HeartRate",
        object? value = null,
        string unit = "Bpm",
        string deviceReported = "2024-01-01T10:00:00Z",
        Guid? measurementId = null
    ) =>
        $$"""
            {
                "measurementId": "{{measurementId ?? Guid.NewGuid()}}",
                "type": "{{type}}",
                "value": {{value ?? 72}},
                "unit": "{{unit}}",
                "deviceReported": "{{deviceReported}}",
                "source": {
                    "deviceId": "garmin-001",
                    "deviceManufacturer": "Garmin",
                    "deviceModel": "Forerunner 255"
                }
            }
            """;

    public static RawBatch MakeRawBatch(string? topic = null, string? payload = null) =>
        new(topic ?? DefaultTopic, payload ?? BatchJson(), DateTime.UtcNow);

    public static IngestionContext MakeContext(string? topic = null, string? payload = null) =>
        new(MakeRawBatch(topic, payload));

    public static DeviceInfo DefaultDeviceInfo() =>
        new()
        {
            DeviceId = "garmin-001",
            DeviceManufacturer = "Garmin",
            DeviceModel = "Forerunner 255",
        };

    public static Measurement MakeMeasurement(
        MeasurementType type = MeasurementType.HeartRate,
        Unit unit = Unit.Bpm,
        double value = 72.0,
        DateTime? deviceReported = null
    ) =>
        new()
        {
            MeasurementId = Guid.NewGuid(),
            MeasurementType = type,
            Value = (float)value,
            Unit = unit,
            DeviceReported = deviceReported ?? DateTime.UtcNow.AddMinutes(-1),
            Source = DefaultDeviceInfo(),
        };

    public static Batch MakeBatch(Guid? patientId = null, List<Measurement>? measurements = null) =>
        new()
        {
            BatchId = Guid.NewGuid(),
            PatientId = patientId ?? DefaultPatientId,
            CorrelationId = DefaultCorrelationId,
            SchemaVersion = 1,
            AppVersion = new Version(1, 0, 0),
            Measurements = measurements ?? [MakeMeasurement()],
            ReceivedAt = DateTime.UtcNow,
        };

    public static MeasurementDto MakeMeasurementDto() =>
        new()
        {
            MeasurementId = Guid.NewGuid(),
            Type = "HeartRate",
            Value = 72.0,
            Unit = "Bpm",
            DeviceReported = DateTime.UtcNow.AddMinutes(-1),
            Source = new DeviceDto
            {
                DeviceId = "garmin-001",
                DeviceManufacturer = "Garmin",
                DeviceModel = "Forerunner 255",
            },
        };

    public static BatchDto MakeBatchDto(
        Guid? patientId = null,
        List<MeasurementDto>? measurements = null
    ) =>
        new()
        {
            SchemaVersion = 1,
            AppVersion = new Version(1, 0, 0),
            PatientId = patientId ?? DefaultPatientId,
            CorrelationId = DefaultCorrelationId,
            Measurements = measurements ?? [MakeMeasurementDto()],
        };
}
