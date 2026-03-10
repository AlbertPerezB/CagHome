namespace CagHome.IngestionService.Domain.Models;

public abstract record DeviceDto
{
    public string? DeviceManufacturer { get; init; }
    public string? DeviceModel { get; init; }
    public string? Platform { get; init; }
}
