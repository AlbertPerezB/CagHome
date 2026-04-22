using CagHome.Simulator;
using CagHome.Simulator.Application;
using CagHome.Simulator.Domain.Profiles;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<SimulatorOptions>(builder.Configuration.GetSection(SimulatorOptions.SectionName));
builder.Services.AddSingleton<ISimulationProfile, NormalSimulationProfile>();
builder.Services.AddSingleton<ISimulationProfile, ExerciseSimulationProfile>();
builder.Services.AddSingleton<ISimulationProfile, ArrhythmiaSimulationProfile>();
builder.Services.AddHostedService<BiometricPublisherService>();

await builder.Build().RunAsync();
