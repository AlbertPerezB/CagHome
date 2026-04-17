using CagHome.Contracts;
using Microsoft.Extensions.Logging;

namespace CagHome.NotificationService.Application.Handlers;

public class HospitalAlertHandler
{
    public Task Handle(HospitalAlertRequested message, ILogger<HospitalAlertHandler> logger)
    {
        logger.LogInformation(
            "Hospital alert received: PatientId={PatientId}, HospitalId={HospitalId}, Severity={Severity}, Message={Message}",
            message.PatientId,
            message.HospitalId,
            message.Severity,
            message.Message
        );

        return Task.CompletedTask;
    }
}
