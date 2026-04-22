namespace CagHome.NotificationService.Infrastructure
{
    public interface IMqttPublisher
    {
        Task Publish(Guid patientId, object payload);
    }
}
