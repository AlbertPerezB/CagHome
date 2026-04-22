namespace CagHome.Simulator.Domain.Models;

public sealed record MeasurementSourcePayload(
    string DeviceId,
    string DeviceManufacturer,
    string DeviceModel);