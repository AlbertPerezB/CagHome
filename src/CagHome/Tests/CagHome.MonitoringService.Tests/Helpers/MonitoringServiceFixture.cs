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

/// <summary>
/// Integration test fixture that hosts MonitoringService components and exposes test stores.
/// </summary>
public class MonitoringServiceFixture : IAsyncLifetime
{
    /// <summary>
    /// Gets the host instance.
    /// </summary>
    public IHost Host { get; private set; } = null!;

    /// <summary>
    /// Gets the patient careplan store.
    /// </summary>
    public PatientCareplanStore PatientCareplanStore { get; private set; } = null!;

    /// <summary>
    /// Gets the in-memory decision audit store.
    /// </summary>
    public DecisionAuditStore DecisionAuditStore { get; private set; } = null!;

    /// <summary>
    /// Initializes the fixture host and registers the dependencies required.
    /// </summary>
    /// <returns>A task when the host has started.</returns>
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

    /// <summary>
    /// Stops and disposes the fixture host.
    /// </summary>
    /// <returns>A task when shutdown is finished.</returns>
    public async Task DisposeAsync()
    {
        await Host.StopAsync();
        Host.Dispose();
    }

    /// <summary>
    /// Clears all in-memory test data from fixture stores.
    /// </summary>
    public void Reset()
    {
        PatientCareplanStore.Clear();
        DecisionAuditStore.Clear();
    }
}