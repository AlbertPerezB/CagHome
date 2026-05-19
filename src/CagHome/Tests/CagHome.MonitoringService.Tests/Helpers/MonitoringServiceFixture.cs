using CagHome.MonitoringService.Application.Decision;
using CagHome.MonitoringService.Application.Decision.Interfaces;
using CagHome.MonitoringService.Application.Decision.Policies;
using CagHome.MonitoringService.Application.Handlers;
using CagHome.MonitoringService.Infrastructure;
using CagHome.MonitoringService.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace CagHome.MonitoringService.Tests.Integration;

public class MonitoringServiceFixture : IAsyncLifetime
{
    
    public IHost Host { get; private set; } = null!;
    public PatientCareplanStore PatientCareplanStore { get; private set; } = null!;
    public DecisionAuditStore DecisionAuditStore { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        PatientCareplanStore = new PatientCareplanStore();
        DecisionAuditStore = new DecisionAuditStore();

        Host = await Microsoft
            .Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseWolverine(options =>
            {
                options.Discovery.IncludeAssembly(typeof(BatchReceivedHandler).Assembly);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<ICareplanDecisionPolicy, NoneCareplanPolicy>();
                services.AddSingleton<ICareplanDecisionPolicy, ValveDiseaseCareplanPolicy>();
                services.AddSingleton<ICareplanDecisionPolicy, CoronaryArteryDiseaseCareplanPolicy>();
                services.AddSingleton<ICareplanDecisionPolicy, CardiomyopathyCareplanPolicy>();
                services.AddSingleton<ICareplanPolicyResolver, CareplanPolicyResolver>();
                services.AddSingleton<ICooldownService, CooldownService>();

                services.AddSingleton(PatientCareplanStore);
                services.AddSingleton(DecisionAuditStore);
                services.AddSingleton<IPatientCareplanStore>(sp =>
                    sp.GetRequiredService<PatientCareplanStore>()
                );
                services.AddSingleton<IDecisionAuditStore>(sp =>
                    sp.GetRequiredService<DecisionAuditStore>()
                );
            })
            .StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Host.StopAsync();
        Host.Dispose();
    }

    public void Reset()
    {
        PatientCareplanStore.Clear();
        DecisionAuditStore.Clear();
    }
}