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
info: Aspire.Hosting.DistributedApplication[0]  
    Now listening on: https://studentcoursereviews.dev.localhost:17062  
Aspire.Hosting.DistributedApplication: Information: Now listening on: https://studentcoursereviews.dev.localhost:17062  
info: Aspire.Hosting.DistributedApplication[0]  
    Login to the dashboard at https://studentcoursereviews.dev.localhost:17062/login?t=6c80f101f90eaaf42ed443f7c73ea3f2  
Aspire.Hosting.DistributedApplication: Information: Login to the dashboard at https://studentcoursereviews.dev.localhost:17062/login?t=6c80f101f90eaaf42ed443f7c73ea3f2  
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
