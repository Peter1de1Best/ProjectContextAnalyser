# Naam project:


# Doel project:
De tool fungeert als een context-exporter die een softwareproject analyseert en vervolgens een door AI bruikbare .txt-file maakt met een beschrijving van het project. Hierbij kan de gebruiker op verschillende manieren beïnvloeden welke delen van het project in de .txt-file voor AI worden opgenomen.

Het doel van het project is om een development project te analyseren qua mappenstructuur, en dan de mappen structuur en/of de inhoud van de files te kunnen afdrukken in 1 grotere .txt file die leesbaar is door ai chatbots als chatgpt en copilot. De intentie is dat de tool die uit dit project stroomt nuttig is voor alle ai chatbots, en dan met name chatbots die gelimiteerd zijn in het aantal files of letters/woorden dat ze kunnen lezen.

Voorlopig zullen de output opties zijn:
- *Structure only* = Alleen projectstructuur & samenvatting inhoud
- *Full export* = Projectstructuur & samenvatting inhoud & file contents
- VOORLOPIG GESCHRAPT: *Selected export* = Alleen geselecteerde files/folders + contents

Hierbij zijn core functionaliteiten dat de tool op basis van invoer in een .json file kan worden geinstrueerd om bepaalde mappen of files te negeren.

Ook moet de tool kunnen worden ingesteld op extensies. Hoewel ik hem schrijf voor het analyseren van een C# project, moet hij met een kleine aanpassing in de .json file kunnen runnen op andere projecten, als je maar aangeeft welke file extensies moeten worden meegenomen.

Een **belangrijke kanttekening** is dat de tool misschien nog wel files met miljarden tekens kan printen (gelimiteerd door de beperkingen van je systeem, zoals werkgeheugen en cpu), maar dat bij een bepaald extreem hoog aantal tokens, de chatbots ook de .txt file niet meer kunnen lezen.

Daarom lijkt het belangrijk om met een toggle aan te kunnen geven wat voor soort output je wilt. Bijvoorbeeld: alleen een analyse van de mappen structuur. Dan zou je, eventueel met behulp van ai, kunnen checken bij enorm grote projecten welke files hij wil zien. En instellen zodat alleen die mappen en files (niet) worden ge-exclude.

## Analyse mappen structuur
Stel dat je mappen structuur dit is:
```
MyProject/
├── Program.cs
├── DataModels.cs
└── Analysis/
    ├── CheckXls.cs
    └── CheckDatabases.cs
```

Dan wil ik dat de tool  die structuur ook teruggeeft als mappen structuur. 
Ook als er sub- subsub en subsubsub mappen (etcetera) zijn moet in principe alle inhoud worden meegenomen. De tool is er niet voor om een oordeel te vellen over hoe complex de mappenstructuur is, maar hopelijk ziet een bouwer als zij/hij/they 10 mappen diep zit dat het project erg complex wordt, en kan zij/hij/they dan zelf beoordelen of deze complexiteit inherent aan het project is of onnodig.
Alle inhoud, behalve de excluded mappen en files natuurlijk.
Verder denk ik dat het goed is om juist wel een .json file te tonen, ook als de extensies alleen .cs voor included beschrijven. Dan toont hij dus wel dat de file er is, maar niet de inhoud ervan.

Bij het beschrijven van de fileinhoud, in het tweede deel van de textfile, is het belangrijk om de **relative filepath** terug te geven, bijvoorbeeld: `FILE: Analysis/CheckXls.cs`.

## Analyse inhoud

Belangrijk is ook hoe een analyse eruit zou moeten zien. Ik stel voor bijvoorbeeld:
```
# PROJECT ANALYSIS
----------------

Files discovered:       87
Files included:         52
Files excluded:         35

## Exclusion summary

- By extension: **aa**
- By directory: bb
- By filename: cc

## Other data
Lines of code:          31,482
Characters:             1,142,381
Estimated tokens:       ~285,000

Largest files:
  DatabaseService.cs       42,301 chars
  ExcelProcessor.cs        38,920 chars
  Program.cs               31,102 chars

Recommended:
  ✓ Full export is reasonable
```

In die analyse moet dus in ieder geval worden gerapporteerd:
- hoeveelheid **discovered** = bestand is tijdens het scannen gevonden (dus alles wat is gevonden)
- hoeveelheid **included** = bestand voldoet aan alle regels en wordt geëxporteerd
- hoeveelheid **excluded** = bestand is gevonden maar uitgesloten door configuratie

Eventueel later in de iteraties nog toevoegen:
- **unsupported/unreadable** = bestand voldoet wel aan de selectie, maar kan niet gelezen worden

```
<b>iteratieve versies</b>
V1  → .cs files + txt
V2  → JSON config
V3  → ignored folders/files
V4  → structure
V5  → token/size estimation
V6  → multiple extensions
V7  → selective export
V8  → export profiles
V9  → GUI
```

``` 
<b>Project preparation (config collection and validation)</b>

JSON inlezen
     ↓
Deserializen
     ↓
Configuration object
     ↓
Validatie
     ↓
┌───────────────┐
│ geldig?       │
└───────────────┘
   ↓         ↓
  ja         nee
   ↓          ↓
 uitvoeren   foutmelding
```

```
<b>Project happy flow</b>

Project folder
       ↓
Scan recursively (door alle onderliggende mappen heen, inclusief submappen, sub-submappen, enzovoort)
       ↓
Discover files
       ↓
Apply extension filter
       ↓
Apply directory exclusions
       ↓
Apply file exclusions
       ↓
Collect metadata
       ↓
Build project structure
       ↓
Calculate statistics
       ↓
Select output mode
       ↓
Generate AI-readable output
       ↓
Save export

```