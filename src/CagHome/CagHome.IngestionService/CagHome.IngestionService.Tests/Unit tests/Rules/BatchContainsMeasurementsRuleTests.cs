using System.Text.Json;
using CagHome.IngestionService.Application.Validation.BatchValidation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;
using CagHome.IngestionService.Infrastructure.Schemas;
using Xunit;

namespace CagHome.IngestionService.Tests.Validation.StructuralValidation;

public class BatchContainsMeasurementsRuleTests
{
    private readonly BatchContainsMeasurementsRule _rule = new();

    private static string ValidPayload() =>
        """
            {
                "schemaVersion": 1,
                "appVersion": "1.0.0",
                "patientId": "a1b2c3d4-0000-0000-0000-000000000000",
                "measurements": [
                    {
                        "measurementId": "bbbbbbbb-0000-0000-0000-000000000000",
                        "type": "HeartRate",
                        "value": 72,
                        "unit": "Bpm",
                        "deviceReported": "2024-01-01T10:00:00Z"
                    }
                ]
            }
            """;

    private static IngestionContext MakeContext(bool containsMeasurements)
    {
        var payload = $$"""
            {
                "schemaVersion": 1,
                "appVersion": "1.0.0",
                "patientId": "{{new Guid()}}",
                "measurements": []
            }
            """;
        var raw = new RawBatch("", payload, DateTime.UtcNow);
        return new IngestionContext(raw)
        {
            Batch = new Batch
            {
                BatchId = Guid.NewGuid(),
                PatientId = patientId,
                SchemaVersion = 1,
                AppVersion = new Version(1, 0, 0),
                Measurements = [],
                ReceivedAt = DateTime.UtcNow,
            },
        };
    }

    [Fact]
    public async Task ValidPayload_ReturnsNull()
    {
        var result = await _rule.ValidateAsync(Parse(ValidPayload()));

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidPayload_WithOptionalSource_ReturnsNull()
    {
        var json = Parse(
            """
            {
                "schemaVersion": 1,
                "appVersion": "1.0.0",
                "patientId": "a1b2c3d4-0000-0000-0000-000000000000",
                "measurements": [
                    {
                        "measurementId": "bbbbbbbb-0000-0000-0000-000000000000",
                        "type": "HeartRate",
                        "value": 72,
                        "unit": "Bpm",
                        "deviceReported": "2024-01-01T10:00:00Z",
                        "source": {
                            "deviceId": "dev-1",
                            "deviceManufacturer": "Garmin",
                            "deviceModel": "Forerunner 255"
                        }
                    }
                ]
            }
            """
        );

        var result = await _rule.ValidateAsync(json);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidPayload_EmptyMeasurements_ReturnsNull()
    {
        var json = Parse(
            """
            {
                "schemaVersion": 1,
                "appVersion": "1.0.0",
                "patientId": "a1b2c3d4-0000-0000-0000-000000000000",
                "measurements": []
            }
            """
        );

        var result = await _rule.ValidateAsync(json);

        Assert.Null(result);
    }

    // --- schemaVersion checks ---

    [Fact]
    public async Task MissingSchemaVersion_ReturnsMissingRequiredField()
    {
        var json = Parse(
            """{ "appVersion": "1.0.0", "patientId": "a1b2c3d4-0000-0000-0000-000000000000", "measurements": [] }"""
        );

        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.MissingRequiredField, result!.Code);
    }

    [Fact]
    public async Task SchemaVersionAsString_ReturnsParseError()
    {
        var json = Parse(
            """{ "schemaVersion": "1", "appVersion": "1.0.0", "patientId": "a1b2c3d4-0000-0000-0000-000000000000", "measurements": [] }"""
        );

        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.ParseError, result!.Code);
    }

    [Fact]
    public async Task UnsupportedSchemaVersion_ReturnsUnsupportedSchemaVersion()
    {
        var json = Parse(
            """{ "schemaVersion": 99, "appVersion": "1.0.0", "patientId": "a1b2c3d4-0000-0000-0000-000000000000", "measurements": [] }"""
        );

        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.UnsupportedSchemaVersion, result!.Code);
    }

    // --- Required top-level fields ---

    [Theory]
    [InlineData("appVersion")]
    [InlineData("patientId")]
    [InlineData("measurements")]
    public async Task MissingTopLevelField_ReturnsInvalidSchema(string field)
    {
        var payload = new Dictionary<string, object>
        {
            ["schemaVersion"] = 1,
            ["appVersion"] = "1.0.0",
            ["patientId"] = "a1b2c3d4-0000-0000-0000-000000000000",
            ["measurements"] = Array.Empty<object>(),
        };
        payload.Remove(field);
        var json = Parse(System.Text.Json.JsonSerializer.Serialize(payload));

        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.InvalidSchema, result!.Code);
    }

    // --- Required measurement fields ---

    [Theory]
    [InlineData("measurementId")]
    [InlineData("type")]
    [InlineData("value")]
    [InlineData("unit")]
    [InlineData("deviceReported")]
    public async Task MissingMeasurementField_ReturnsInvalidSchema(string field)
    {
        var measurement = new Dictionary<string, object>
        {
            ["measurementId"] = "bbbbbbbb-0000-0000-0000-000000000000",
            ["type"] = "HeartRate",
            ["value"] = 72,
            ["unit"] = "Bpm",
            ["deviceReported"] = "2024-01-01T10:00:00Z",
        };
        measurement.Remove(field);

        var payload = new
        {
            schemaVersion = 1,
            appVersion = "1.0.0",
            patientId = "a1b2c3d4-0000-0000-0000-000000000000",
            measurements = new[] { measurement },
        };
        var json = Parse(System.Text.Json.JsonSerializer.Serialize(payload));

        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.InvalidSchema, result!.Code);
    }

    // --- Type violations ---

    [Fact]
    public async Task MeasurementValueAsString_ReturnsInvalidSchema()
    {
        var json = Parse(
            """
            {
                "schemaVersion": 1,
                "appVersion": "1.0.0",
                "patientId": "a1b2c3d4-0000-0000-0000-000000000000",
                "measurements": [
                    {
                        "measurementId": "bbbbbbbb-0000-0000-0000-000000000000",
                        "type": "HeartRate",
                        "value": "seventy-two",
                        "unit": "Bpm",
                        "deviceReported": "2024-01-01T10:00:00Z"
                    }
                ]
            }
            """
        );

        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.InvalidSchema, result!.Code);
    }

    [Fact]
    public async Task InvalidDateTimeFormat_ReturnsInvalidSchema()
    {
        var json = Parse(
            """
            {
                "schemaVersion": 1,
                "appVersion": "1.0.0",
                "patientId": "a1b2c3d4-0000-0000-0000-000000000000",
                "measurements": [
                    {
                        "measurementId": "bbbbbbbb-0000-0000-0000-000000000000",
                        "type": "HeartRate",
                        "value": 72,
                        "unit": "Bpm",
                        "deviceReported": "not-a-date"
                    }
                ]
            }
            """
        );

        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.InvalidSchema, result!.Code);
    }

    // --- IsFatal ---

    [Fact]
    public void IsFatal_IsTrue()
    {
        Assert.True(_rule.IsFatal);
    }
}
