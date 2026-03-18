namespace CagHome.IngestionService.Domain.Models;

public record DeviceInfo
{
    public string? DeviceManufacturer { get; init; }
    public string? DeviceModel { get; init; }
    public string? DeviceId { get; init; }

    public DeviceInfo(DeviceDto dto)
    {
        DeviceManufacturer = dto.DeviceManufacturer;
        DeviceModel = dto.DeviceModel;
        DeviceId = dto.DeviceId;
    }

    public DeviceInfo()
    {
        DeviceManufacturer = "unknown";
        DeviceModel = "unknown";
        DeviceId = "unknown";
    }
}
