namespace CagHome.IngestionService.Domain.Models;

public abstract record SourceInfo
{
    public string? DeviceManufacturer { get; init; }
    public string? DeviceModel { get; init; }
    public string? Platform { get; init; }
}
