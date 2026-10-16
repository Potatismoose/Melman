# Melman ⭐

Ett labyrintäventyr till Melwin, byggt i C# för **Unity 6000.6.6f1**.
Samla stjärnorna, undvik spökena och slå ditt rekord!

## Spela i Unity

1. Installera Unity Hub och Unity **6000.6.6f1**, med byggstöd för din målplattform.
2. Lägg till den här projektmappen i Hub och öppna den. Aktivera Unity med din egen giltiga licens vid behov.
3. Första importen skapar scenen `Assets/Melman/Scenes/Melman.unity` och lägger den i bygglistan.
4. Tryck **Play**. Välj svårighetsgrad och tryck **Spela**.

Om en annan scen är öppen: välj **Melman → Create or open game scene**.
Bygg ett fristående spel via **File → Build Profiles**. Välj Melman-scenen och målplattform.

| Kontroll | Funktion |
| --- | --- |
| Piltangenter / WASD | Flytta; figuren fortsätter i vald riktning. Tryck nästa sväng i förväg. |
| Tab | Växla mellan rak 2D-vy och förstapersonsvy i 3D |
| P / Esc | Pausa eller fortsätt |

3D-läget har en 2D-minikarta med väggar, stjärnor, Melman och spöken.
Kameran sitter i Melmans ögonhöjd och följer mjukt genom korridorerna.
I förstapersonsläget betyder W/↑ framåt, A/← sväng vänster, D/→ sväng höger
och S/↓ vänd om. Svängarna kan köas före korsningar. Kameran följer den
faktiska färdriktningen; håll inte in en tangent för upprepade svängar.
Minikartan är alltid nordorienterad, med en vit riktningsmarkör vid Melman.
Tab återgår till översikten, där tangenterna följer kartans riktningar.

Rörelser visas mjukt mellan rutorna. Nästa sväng sparas tills vägen öppnar sig,
även om du släpper tangenten. En ny riktning ersätter den köade svängen.

## Regler

- Fem nya, sammanhängande labyrinter per spelomgång. Nästa bana blir snabbare.
- Lätt: två långsammare spöken. Normal: tre spöken. Svår: tre snabbare spöken.
- Tre liv. Stjärna: 10 poäng, kraftkula: 50, sårbart spöke: 200.
- Gröna kraftkulor ger tillfälligt skydd. Samla alla stjärnor och kraftkulor för att gå vidare.
- Banbonus: 250 × bannumret. Alla fem banor klarade ger vinst.
- Rekordet sparas lokalt med Unity PlayerPrefs, gemensamt för svårighetsgraderna.

Projektet använder Built-in Render Pipeline och det klassiska Input Manager-systemet.
Under **Edit → Project Settings → Player → Active Input Handling**, välj
**Input Manager (Old)** eller **Both** om Unity ber om ett val.

## Utveckling och tester

`Assets/Melman/Core/Game.cs` innehåller spellogiken utan Unity-beroenden.
`Assets/Melman/Runtime/MelmanApp.cs` skapar grafik, kameror, menyer och input vid körning.
`Assets/Melman/Editor/ProjectSetup.cs` skapar startscenen och innehåller en Linux-byggmetod.
Inga externa grafikpaket eller speltjänster krävs. Grafiken använder Unity-primitiver.

Installera .NET SDK 8 och kör från projektmappen:

```sh
./scripts/test.sh
```

Testprogrammet avslutas med felkod vid misslyckat test. Det verifierar 90 slumpfrön över tre
svårighetsgrader, nåbarhet, fem banors progression, poäng, kollisioner, kraftkulor och slutlägen.
Det testar inte Unity-rendering, input eller PlayerPrefs.

Med en installerad och licensierad Unity-editor samt Linux Build Support:

```sh
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD" \
  -executeMethod Melman.Editor.ProjectSetup.BuildLinux -logFile /tmp/melman-build.log
```

## Status i molnmiljön

.NET SDK 8.0.426 är installerat utanför repot i `/workspace/tooling/dotnet`.
Spellogikens tester har körts: **18 676 kontroller godkända**.
Unity-editor och Unity-licens saknas i denna miljö. Unity-kompilering, spelstart,
kameror, input, rekordsparning och fristående bygge är därför **inte verifierade**.
Öppna projektet i Unity enligt instruktionerna ovan för nästa kontroll.

Första versionen har lokal enspelarstyrning med tangentbord. Ljud, mobilstyrning,
online-topplista och egenritad grafik ingår inte ännu.
