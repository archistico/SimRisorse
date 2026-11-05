# RC76-HF02 — Contratto pubblico `CityCatalog`, piano di validazione

## Errore osservato

Dopo la hotfix RC76-HF01, `dotnet` compilava Core, Application, Simulation e Infrastructure ma non Desktop, Application.Tests e Infrastructure.Tests: 8 errori CS1061, tutti riferiti a `V15FoodIndustryCampaignSession.CityCatalog`. La classe di proiezione `V15CampaignCatalogProjection` e il record `V15CampaignCatalogView` erano presenti, ma il punto di accesso sulla sessione era scomparso.

## Correzione

Nel file `src/SimRisorse.Core/Food/V15FoodIndustryCampaignSession.cs` è stata reintrodotta la proprietà pubblica `V15CampaignCatalogView CityCatalog`, che restituisce `V15CampaignCatalogProjection.Project(_baseline, _current, _catalog)`. È una proiezione di sola lettura, sempre aggiornata in seguito a un `Commit`, senza cache ed effetti sullo stato. Non si modificano gli otto call site già esistenti, né i file Desktop o XAML.

## Prove richieste su Windows con .NET 10

```powershell
.\validate.cmd
.\validate-f1.cmd
.\run-game.cmd
```

**Atteso:** compilazione dei progetti Core/Application/Infrastructure/Desktop e dei relativi test, 869/869 xUnit superati senza ignorati, gate F1 con audit dei profili baseline e combined e smoke Desktop. Controllare visivamente la GUI v15: edifici bloccati grigi consultabili, dettagli iconici in elenco, pulsanti nel layout storico, progresso popolazione nella sidebar, tabella Risorse a sette colonne e Scuola/Esercito.

I test esistenti `F1D05d4aCityCommandsTests`, `F1D05e2b2V15NativeRosterTests` e `F1D05d4aCityCommandsPersistenceTests`, assieme al ViewModel, utilizzano già `CityCatalog`: non sono stati aggiunti nuovi test perché il compilatore controlla già la presenza del contratto in tutti e otto i punti. Eventuali test falliti **dopo** la compilazione vanno diagnosticati separatamente.

## Verifiche locali e limiti

Verificati staticamente l'esistenza del record e del metodo di proiezione, la firma della proprietà, i call site e l'integrità del pacchetto. La compilazione, l'esecuzione dei test e il gate F1 **non sono eseguibili in questo ambiente**, dove manca l'SDK .NET 10. Stato: candidata, non validata.
