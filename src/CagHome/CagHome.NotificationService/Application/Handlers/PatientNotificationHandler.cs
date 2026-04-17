using CagHome.Contracts;
using Microsoft.Extensions.Logging;

namespace CagHome.NotificationService.Application.Handlers;

public class PatientAlerHandler
{
    public Task Handle(PatientAlertRequested message, ILogger<PatientAlertRequested> logger)
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
