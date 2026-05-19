namespace CagHome.Simulator.Domain.Models;

public record MeasurementSourcePayload(
    string DeviceId,
    string DeviceManufacturer,
    string DeviceModel);