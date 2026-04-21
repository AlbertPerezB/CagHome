using CagHome.Contracts;
using CagHome.Contracts.Enums;

namespace CagHome.EhrIntegrationService.Infrastructure
{
    public interface IRabbitMqPublisher
    {
        Task PublishClinicianResponseReceived(ClinicianResponseReceived message);
        Task PublishPatientStatusUpdateRequested(PatientStatusUpdateRequested message);
        Task PublishCareplanUpdateRequested(CareplanUpdateRequested message);
    }
}
