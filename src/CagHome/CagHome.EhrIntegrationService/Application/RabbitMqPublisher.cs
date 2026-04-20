using CagHome.Contracts;
using CagHome.Contracts.Enums;
using Wolverine;

namespace CagHome.EhrIntegrationService.Application
{
    public class RabbitMqPublisher(IServiceScopeFactory serviceScopeFactory) : IRabbitMqPublisher
    {
        public async Task PublishClinicianResponseReceived(ClinicianResponseReceived message)
        {
            var bus = getBus();
            await bus.PublishAsync(message);
        }

        public async Task PublishPatientStatusUpdateRequested(PatientStatusUpdateRequested message)
        {
            var bus = getBus();
            await bus.PublishAsync(message);
        }

        public async Task PublishCareplanUpdateRequested(CareplanUpdateRequested message)
        {
            var bus = getBus();
            await bus.PublishAsync(message);
        }

        private IMessageBus getBus()
        {
            using var scope = serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IMessageBus>();
        }
    }
}
