# RC55 — F1-D05d.4b.1: correzione CS0103 e gate Windows

## Evidenza di partenza

Sul runner Windows RC54 il progetto `SimRisorse.Desktop` non compila per **due errori CS0103**, entrambi in `V15PreviewViewModel.cs` nelle clausole di gestione di `GameSaveException` di `Save()` e `Load()`. La classe esiste già in `SimRisorse.Application.Persistence` (`IGameSaveStore.cs`); il ViewModel v14 importa già lo stesso namespace.

## Patch

Nel solo file `src/SimRisorse.Desktop/Presentation/V15PreviewViewModel.cs` aggiungere `using SimRisorse.Application.Persistence;`. Non togliere i catch e non sostituire `GameSaveException` con `Exception` generica. Nessun nuovo test: gli 8 test Desktop RC54 sono ancora da eseguire, **713 totali attesi**. Verificare anche che `CorruptV15CheckpointBlocksAutosaveInsteadOfBeingReplaced` superi e che non si tocchi il database v14.

## Comandi Windows

```powershell
.\validate.cmd

dotnet test tests\SimRisorse.Desktop.Tests\SimRisorse.Desktop.Tests.csproj -c Release --filter FullyQualifiedName~V15DesktopIntegrationTests

.\run-game.cmd --v15
```

Non eseguire una GUI su una build non verde. Se emergono nuovi errori, usare il nuovo log/TRX senza inferire che tutti i 713 test siano passati.

## Esiti attuali

- **RC53:** 705 test realmente passati (conferma utente).
- **RC54:** 2 errori di compilazione C#; gate dei test non completato.
- **RC55:** import aggiunto, controlli statici/manifest ZIP verificabili in preparazione; compilazione e xUnit .NET 10 non eseguiti in questo ambiente.

## Prossimo passo dopo gate verde

F1-D05d.4b.2: proiezione dei flussi e dell'ETA nella tabella Risorse v15 a sette colonne; poi F1-D05e, E4a ed E4b, senza cambiare anticipatamente le formule economiche.
