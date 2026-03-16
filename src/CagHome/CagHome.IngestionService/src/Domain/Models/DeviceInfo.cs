namespace CagHome.IngestionService.Domain.Models;

public record DeviceInfo
{
    public string? DeviceManufacturer { get; init; }
    public string? DeviceModel { get; init; }
    public string? Platform { get; init; }

    public DeviceInfo(DeviceDto dto)
    {
        DeviceManufacturer = dto.DeviceManufacturer;
        DeviceModel = dto.DeviceModel;
        Platform = dto.Platform;
    }

    public DeviceInfo()
    {
        DeviceManufacturer = "unknown";
        DeviceModel = "unknown";
        Platform = "unknown";
    }
}
