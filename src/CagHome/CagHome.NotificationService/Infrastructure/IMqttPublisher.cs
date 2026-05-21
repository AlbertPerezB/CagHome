namespace CagHome.NotificationService.Infrastructure
{
    /// <summary>
    /// Defines a contract for publishing messages to an MQTT broker for a specific patient.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for serializing the payload and handling
    /// the details of message delivery. Thread safety and delivery guarantees depend on the concrete
    /// implementation.</remarks>
    public interface IMqttPublisher
    {
        Task Publish(Guid patientId, object payload);
    }
}
