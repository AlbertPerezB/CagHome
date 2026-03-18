# CAG-Home
Description here

## Setup
Install the following:
- [Visual Studio Code](https://code.visualstudio.com/download)
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Aspire](https://aspire.dev/get-started/install-cli/)
- [Aspire VS Code extension](https://aspire.dev/get-started/aspire-vscode-extension/)

## Start the project
In the **Debug** tab in the left sidebar:
Select **Launch AppHost (Aspire)** and press **Run**.  
The project will be built and started.  
At some point, you should see this in the debug console:

```
Starting dashboard...
Now listening on: https://localhost:17182
AppHost:  src\CagHome\CagHome.AppHost\CagHome.AppHost.csproj
Logs:  C:\Users\Albert\.aspire\cli\logs\apphost-29688-2026-03-18-11-36-07.log
Dashboard: https://localhost:17182/login?t=d445f2ce56d03ed929f7da56a3078ca9
Login to the dashboard at https://localhost:17182/login?t=d445f2ce56d03ed929f7da56a3078ca9
Distributed application started. Press Ctrl+C to shut down.
```

Click the login link if the page does not open automatically. 

## Simulator configuration
The simulator project is `src/CagHome/CagHome.Simulator`.

### Configure in appsettings
Update the `Simulator` section in:
- `src/CagHome/CagHome.Simulator/appsettings.json`

Example:

```json
{
    "Simulator": {
        "Profile": "normal",
        "DeviceCount": 3,
        "PublishIntervalSeconds": 2
    }
}
```

Supported profile values are:
- `normal`
- `exercise`
- `arrhythmia`

If an invalid profile value is entered, the simulator defaults to `normal`.

### Change profile at runtime
The simulator uses runtime configuration reloading. While it is running, you can change `Simulator:Profile` in `appsettings.json` and save the file.

On the next publish cycle, the simulator picks up the new profile without restarting the process.
