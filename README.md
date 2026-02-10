Det er smart at have en broker, fordi den kan være det første kunderne møder. Brug den til verficere login / auth. Men også da den som broker kan smide forskellige beskeder til forskellige consumere. Hvis der er mange beskeder til consumer eller vi ændre navne på klasser sygdomme whatever, så rammer alle beskeder samme endpoint og vi kan dele det ud til alle mulige consumere / mqtt consumere på bagsiden. Fungerer som en gateway. 

Brokerne kan også som det første smide correlation id på alt og vi har overblik i logs/traces.

Broker kan ik være bottleneck, man kan bare lave flere. 

Sørger for at klienterne har samme endpoint alle sammen. 

Mulighed for at skalere på bagsiden da vi kan styre hvor alt kommer hen. Vi kan styre hvor mange instanster af consumers for hver beskedtype baseret på load. 

Så har vi x antal consumere på bagsiden, skal de gøre noget eller bare kommunikere besked til mq. Consumer = ingestion services, masser af dem. De skal nok gemme dataen og sende besked om at data modtaget via mq, overvej WolverineFX. Til nem eventdreven ja. Det et lag oven på mq. Til at håndtere alt det besværlige. Kig på det. 

Kig evt på redis for cache