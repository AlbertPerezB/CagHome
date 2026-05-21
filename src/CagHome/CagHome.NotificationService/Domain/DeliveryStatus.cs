namespace CagHome.NotificationService.Domain
{
    /// <summary>
    /// Specifies the possible outcomes of a delivery operation.
    /// </summary>
    public enum DeliveryStatus
    {
        Attempted,
        Delivered,
        Failed,
    }
}
