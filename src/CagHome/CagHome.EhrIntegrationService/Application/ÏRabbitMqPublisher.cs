using CagHome.Contracts;
using CagHome.Contracts.Enums;

namespace CagHome.EhrIntegrationService.Application
{
    public interface IRabbitMqPublisher
    {
        Task PublishClinicianResponseReceived(ClinicianResponseReceived message);
        Task PublishPatientStatusUpdateRequested(PatientStatusUpdateRequested message);
        Task PublishCareplanUpdateRequested(CareplanUpdateRequested message);
    }
}
