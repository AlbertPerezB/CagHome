namespace CagHome.IngestionService.Domain.Models;

public abstract record DeviceInfo
{
    public string? DeviceManufacturer { get; init; }
    public string? DeviceModel { get; init; }
    public string? Platform { get; init; }
}
