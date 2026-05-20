using CagHome.IngestionService.Domain.Enums;
using CagHome.IngestionService.Domain.Models;

namespace CagHome.IngestionService.Application.Validation.MeasurementValidation;

/// <summary>
/// Validation rule to ensure that measurements have the correct unit for their type.
/// For example, a heart rate measurement should have a unit of bpm, and a body temperature
/// measurement should have a unit of either C or F.
/// </summary>
public class CorrectUnitRule : IValidationRule<Measurement>
{
    private static readonly Dictionary<MeasurementType, HashSet<Unit>> AllowedUnits = new()
    {
        {
            MeasurementType.HeartRate,
            new() { Unit.Bpm }
        },
        {
            MeasurementType.Spo2,
            new() { Unit.Percent }
        },
        {
            MeasurementType.BodyTemperature,
            new() { Unit.C, Unit.F }
        },
    };

    public async Task<ValidationError?> ValidateAsync(Measurement input)
    {
        if (
            !AllowedUnits.TryGetValue(input.MeasurementType, out var allowed)
            || !allowed.Contains(input.Unit)
        )
        {
            var error = new ValidationError(
                ValidationCode.InvalidUnit,
                $"Measurement of type {input.MeasurementType} has invalid unit {input.Unit}"
            );

            return error;
        }

        return null;
    }
}
