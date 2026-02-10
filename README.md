# CAG-Home
Beskrivelse her

## Opsætning
Skal have det her installeret:
- [Visual Studio Code](https://code.visualstudio.com/download)
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [.net 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Aspire ](https://aspire.dev/get-started/install-cli/)
- [Aspire vscode extension](https://aspire.dev/get-started/aspire-vscode-extension/)

## Start projektet
På "debug" tab'en i venstre sidebar.  
Vælg Launch AppHost (Aspire) og hit run  
Projektet bliver bygget og startet.  
På et tidspunkt i debug console vinduet ses:

```
info: Aspire.Hosting.DistributedApplication[0]  
    Now listening on: https://studentcoursereviews.dev.localhost:17062  
Aspire.Hosting.DistributedApplication: Information: Now listening on: https://studentcoursereviews.dev.localhost:17062  
info: Aspire.Hosting.DistributedApplication[0]  
    Login to the dashboard at https://studentcoursereviews.dev.localhost:17062/login?t=6c80f101f90eaaf42ed443f7c73ea3f2  
Aspire.Hosting.DistributedApplication: Information: Login to the dashboard at https://studentcoursereviews.dev.localhost:17062/login?t=6c80f101f90eaaf42ed443f7c73ea3f2  
```

Tryk på login linket

## Notes

[Mongo](https://www.mongodb.com/resources/products/capabilities/mongodb-time-series-data) til timeseries. [Mongo Timeseries docs](https://www.mongodb.com/docs/manual/core/timeseries-collections/)

Det er smart at have en broker, fordi den kan være det første kunderne møder. Brug den til verficere login / auth. Men også da den som broker kan smide forskellige beskeder til forskellige consumere. Hvis der er mange beskeder til consumer eller vi ændre navne på klasser sygdomme whatever, så rammer alle beskeder samme endpoint og vi kan dele det ud til alle mulige consumere / mqtt consumere på bagsiden. Fungerer som en gateway. 

Brokerne kan også som det første smide correlation id på alt og vi har overblik i logs/traces.

Broker kan ik være bottleneck, man kan bare lave flere. 

Sørger for at klienterne har samme endpoint alle sammen. 

Mulighed for at skalere på bagsiden da vi kan styre hvor alt kommer hen. Vi kan styre hvor mange instanster af consumers for hver beskedtype baseret på load. 

Så har vi x antal consumere på bagsiden, skal de gøre noget eller bare kommunikere besked til mq. Consumer = ingestion services, masser af dem. De skal nok gemme dataen og sende besked om at data modtaget via mq, overvej WolverineFX. Til nem eventdreven ja. Det et lag oven på mq. Til at håndtere alt det besværlige. Kig på det. 

Kig evt på redis for cache
