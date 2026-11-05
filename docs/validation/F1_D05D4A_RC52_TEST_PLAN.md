# SimRisorse RC52 — Piano di test F1-D05d.4a

**Gate:** implementazione headless conclusa; **compilazione e suite xUnit .NET 10 non eseguite in questo ambiente**. RC47 (620) è l'ultimo checkpoint validato dall'utente; RC48 +11, RC49 +17, RC50 +19, RC51 +16 e RC52 **+20 test nuovi** => **703 test indicativi**, da verificare con `validate.cmd`. Non confondere conteggio dei `[Fact]` con test effettivamente scoperti.

## Comandi Windows

Nella root della versione estratta:

```powershell
.\validate.cmd

dotnet test tests/SimRisorse.Application.Tests/SimRisorse.Application.Tests.csproj -c Release --filter FullyQualifiedName~F1D05d4aCityCommandsTests

dotnet test tests/SimRisorse.Infrastructure.Tests/SimRisorse.Infrastructure.Tests.csproj -c Release --filter FullyQualifiedName~F1D05d4aCityCommandsPersistenceTests
```

## Matrice aggiunta RC52

| Controllo | Criterio |
| --- | --- |
| Catalogo v15 | 6 siti alimentari e 3 tier distinti; `food-store` invisibile, `food-depot` presente una sola volta |
| Risorse v15 | Cibo = stock del Deposito e relativa capacità; Pesce, Pane, Carne, Frutta, Verdura e animali sommati fra i proprietari fisici senza doppio conteggio |
| Costruzioni | Costi e storia di acquisto preservati per civili/tier; nuovo sito alimentare nello stesso checkpoint |
| Residui legacy | Non acquistare/assegnare il vecchio produttore; decommissionamento esplicito idempotente senza perdita di risorse |
| Scuola | Prerequisito nuovo Deposito; iscrizione paga Cibo pronto, rimborso riaccredita Cibo al Deposito senza superare capacità |
| Matite | Formazione con Matite e restituzione corretta su annullamento, senza duplicazioni |
| Personale | Le nuove filiere sottraggono persone a industria legacy, Scuola e militari |
| Militare | Caserma obbligatoria, requisiti spade, annullamento con rimborso, no doppia riserva |
| Ricerca | Punti spesi solo a richiesta, mai Cibo spostato nella città |
| Clock | Dopo iscrizione, Scuola ricerca ed esercito avanzano nello stesso `Advance` già presente |
| SQLite | Salvataggio dopo azioni miste, ricarico, prosecuzione del clock e confronto; soppressione definitiva del vecchio produttore conservata |
| Comandi non validi | Ogni rifiuto restituisce causa tipizzata e non muta il checkpoint |
| GUI v14 | File XAML, ViewModel e persistenza normale invariati; verificare `run-game.cmd` senza modificare il salvataggio personale |

## Verifiche supplementari prima della GUI v15

1. Eseguire tutti i test con avvisi trattati come errori: correggere prima eventuali regressioni RC48–52.
2. Eseguire una campagna headless con assegnazioni massime, ricercatori, addestramento e più livelli, ripetendo `Save→Load→Advance`.
3. Controllare in un DB **di prova** l'assenza di risorsa `food` nella tabella delle risorse cittadine dopo l'iscrizione alla Scuola.
4. Verificare la resa v14 della tabella Risorse classica a sette colonne e la selezione dei livelli come righe distinte. Nessun test Desktop nuovo è stato aggiunto per RC52 perché non è stata modificata la GUI.
5. Non usare `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3` per provare SQLite v15.

[Specifica](../design/F1_D05D4A_RC52_COMANDI_CATALOGO_V15.md) · [Roadmap](../ROADMAP.md) · [ADR 0063](../adr/0063-comandi-v15-cibo-esclusivo-e-catalogo-di-presentazione.md).
