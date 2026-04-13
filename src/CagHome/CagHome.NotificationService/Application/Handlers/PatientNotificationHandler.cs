using CagHome.Contracts.Notifications;
using Microsoft.Extensions.Logging;

namespace CagHome.NotificationService.Application.Handlers;

public class PatientNotificationHandler
{
    public Task Handle(
        PatientNotificationRequested message,
        ILogger<PatientNotificationHandler> logger
    )
    {
        logger.LogInformation(
            "Patient notification received: PatientId={PatientId}, Severity={Severity}, Message={Message}",
            message.PatientId,
            message.Severity,
            message.Message
        );

        return Task.CompletedTask;
    }
}
