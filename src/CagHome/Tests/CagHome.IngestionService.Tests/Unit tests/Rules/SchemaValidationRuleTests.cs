using System.Text.Json;
using CagHome.IngestionService.Application.Validation.StructuralValidation;
using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Infrastructure.Schemas;

namespace CagHome.IngestionService.Tests.UnitTests;

public class SchemaValidationRuleTests
{
    private readonly SchemaValidationRule _rule = new(new JsonSchemaRegistry());

    private static JsonDocument Parse(string json) => JsonDocument.Parse(json);

    [Fact]
    public async Task ValidPayload_ReturnsNull()
    {
        var result = await _rule.ValidateAsync(Parse(TestDataFactory.ValidJsonPayload()));

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidPayload_EmptyMeasurements_ReturnsNull()
    {
        var json = Parse(TestDataFactory.BatchJson(measurements: Array.Empty<string>()));

        var result = await _rule.ValidateAsync(json);

        Assert.Null(result);
    }

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
        var json = Parse(TestDataFactory.BatchJson(schemaVersion: 99));
        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.UnsupportedSchemaVersion, result!.Code);
    }

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
        var json = Parse(JsonSerializer.Serialize(payload));

        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.InvalidSchema, result!.Code);
    }

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
        var json = Parse(JsonSerializer.Serialize(payload));

        var result = await _rule.ValidateAsync(json);

        Assert.NotNull(result);
        Assert.Equal(ValidationCode.InvalidSchema, result!.Code);
    }

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

    [Fact]
    public void IsFatal_IsTrue()
    {
        Assert.True(_rule.IsFatal);
    }
}
