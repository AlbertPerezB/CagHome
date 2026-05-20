namespace CagHome.MockApplication.Domain.Models;

/// <summary>
/// Describes the device metadata for a measurement.
/// </summary>
/// <param name="DeviceId">Unique identifier of the source device.</param>
/// <param name="DeviceManufacturer">Manufacturer name of the source device.</param>
/// <param name="DeviceModel">Model name or number of the source device.</param>
public record MeasurementSourcePayload(
    string DeviceId,
    string DeviceManufacturer,
    string DeviceModel);