# RC76-HF01 — Piano di verifica del fix CS0117

## Diagnosi confermata

`V15FoodCampaignCheckpoints.BusyOutsideFood(V15CityCampaignSnapshot)` è `internal` nel progetto `SimRisorse.Core`, quindi il progetto `SimRisorse.Application` non può richiamarlo. I due riferimenti originali erano nei benchmark naturale e industriale v15, nelle funzioni `Sample`. Il fallimento di `validate-f1.cmd` è una conseguenza del blocco di compilazione: non è un fallimento dei test xUnit o dell'audit fisico.

## Modifica realizzata

- Introdotta l'API **pubblica** `V15FoodCampaignCheckpoints.AvailableWorkers(V15FoodCampaignCheckpoint)`; il calcolo usa `BusyOutsideFood` (famiglie industriali, altri edifici, scuola e militari), più il personale delle sei nuove filiere, sottratti dalla popolazione.
- I due benchmark richiamano soltanto l'API pubblica; nessuna copia della formula in Application.
- `V15FoodIndustryCampaignSession.AvailableWorkers` delega allo stesso metodo, così i report e il gameplay mantengono un'unica fonte di verità.
- `E4aV15WorkforceProjectionRegressionTests`: 2 test nuovi, uno sul checkpoint iniziale e uno sullo scenario combinato da 24 abitanti, 22 persone occupate e 2 libere.

## Verifiche su Windows/.NET 10

Eseguire dalla radice del progetto:

```powershell
.\validate.cmd
.\validate-f1.cmd
```

**Atteso:** 869/869 test verdi, nessun test ignorato. Il gate F1 richiede inoltre smoke della GUI e due audit v15 (baseline e combined). L'ispezione visiva della GUI rimane separata e obbligatoria per chiudere F1-D05e.

Se la compilazione dovesse ancora fallire, allegare il primo errore C# effettivo, non soltanto l'ultima riga `dotnet failed` del wrapper PowerShell.

## Limiti della verifica effettuata in questa consegna

- Controlli statici su firme, richiami, file XML/JSON, riferimenti Markdown e ZIP con manifest SHA-256.
- **Non** eseguita compilazione o suite xUnit .NET 10 (SDK assente); il numero 869 è il totale **atteso**, non certificato.
- Nessuna modifica a XAML/Desktop, bilanciamento, cataloghi, schema SQLite generation 6, salvataggi v14/v15 o clock di produzione.
