# RC80 — Audit statico del pacchetto completo (06/11/2026)

**Tipo di evidenza: statica, NON validazione runtime.** Confronto condotto sui file presenti in `SimRisorse_RC79_E4b2a_candidato.zip`, usato come base. La RC79 conserva 17 test non verificati; non si attribuiscono mai esiti positivi a RC79/RC80 senza esecuzione .NET.

## Esito del confronto prima del confezionamento

- **806 file originali**, nessuno eliminato, **9 file aggiunti** (3 Core/Application, 2 test, 1 CLI, 3 documenti); **11 file modificati** (planner, dispatch CLI, gate e documentazione), per un totale di **815 file** prima del presente audit.
- Nessun file modificato in `src/SimRisorse.Desktop`, `src/SimRisorse.Infrastructure`, `src/SimRisorse.Core/Food`, `src/SimRisorse.Core/Sessions` o nelle specifiche SQLite. Nessun DB alterato, ricostruito o cancellato.
- Bilanciamento dei delimitatori C# verificato sui 7 sorgenti/test della RC80 e su `Program.cs` dopo aver rimosso token stringa/commento. **Questo non equivale a compilare il codice.**
- Suite nuova esaminata: 10 `[Fact]` Core, 8 casi `[InlineData]` Core, 4 `[Fact]` Application = **22 test nuovi**. Totale atteso con RC78 (926) e RC79 (17): **965**. `scripts/validate-f1-gate.ps1` richiede esattamente 965 esiti senza saltati e verifica anche tre output della CLI RC80.
- Gate di gioco e regressioni visuali **non eseguiti**: nella macchina di preparazione non sono disponibili SDK .NET 10 o `pwsh`/PowerShell. Nessun report TRX o screenshot creato per RC80.

## Criticità residua prima di abilitare la Zecca in partita

Il portafoglio attuale usa 1/3/10 posti per tier. Non è coerente con 10/30/100 persone per l'industria Zecca, pertanto la proiezione RC80 rimane isolata. Collegarla al gioco senza una transizione dei comandi, della capacità, dello snapshot e della contabilità della popolazione produrrebbe errori. Restano da decidere la curva economica, il pagamento progressivo, i limiti di magazzino e la tipizzazione delle squadre per le altre famiglie. È vietato considerare 10 fixture controllate una prova di raggiungibilità naturale.

## Criterio di promozione

Eseguire quando disponibile una postazione Windows con SDK .NET 10:

```powershell
.\validate-f1.cmd
```

Rivedere l'evidenza fresca `.local-cache/m4/f1-gates/<id>/f1-gate.json`: esigere 965/965 test, 0 falliti e 0 ignorati, audit e smoke già previsti più `e4b-crews.json/csv/md`. Completare separatamente il controllo manuale della GUI v15 rispetto alla v14 canonica; nessuna testata automatica lo sostituisce.
