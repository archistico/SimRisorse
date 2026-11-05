# RC47 — Audit statico e differenze rispetto alla RC46

**Data:** 2026-11-05. **Gate Windows:** non eseguito: SDK .NET 10 non disponibile in questo ambiente.

## Confronto manifest (RC46 → RC47)

- **2 nuovi file C#**: `src/SimRisorse.Infrastructure/Persistence/V14ToV15FoodMigrator.cs` e `tests/SimRisorse.Infrastructure.Tests/V14ToV15FoodMigrationTests.cs`.
- **0 file C# esistenti modificati**; 0 file preesistenti eliminati. Nessuna modifica a GUI, simulatore, Core, wrapper di salvataggio v14/v15, script, cataloghi, coefficienti o SQL attivo.
- Nuovi documenti: design F1-D05c, piano test, ADR 0058 e questo audit; aggiornati README, CHANGELOG, roadmap, stato, handoff, registro decisioni, indice e guida dei salvataggi.
- **11 nuovi metodi `[Fact]`** nel test di migrazione; 609 test attesi RC46 + 11 = **620 attesi RC47**; non certificati mediante esecuzione.

## Controlli riproducibili

- XML/XAML e file `.slnx/.csproj/.props`: 26 documenti XML analizzati senza errori.
- File `.json`: 39 analizzati senza errori.
- Collegamenti Markdown locali: 1.629 risolti, nessun riferimento rotto prima dell'inserimento di questo audit.
- Verifica finale ZIP con lettura completa e `ZipFile.testzip()` e confronto dei percorsi all'atto della consegna.

## Limiti deliberati

- I test xUnit della RC46 e RC47 **non sono stati eseguiti**: l'ultimo risultato confermato è RC45 (600/600).
- Migrazione disponibile solo per chiamanti che richiamino esplicitamente l'API Infrastructure; non collegata al gioco; nessuna nuova partita v15 caricabile dal launcher.
- La protezione da modifiche concorrenti richiede che il gioco sia chiuso. Per writer con WAL attivo sono necessari un gate di quiescenza e prove di crash/recovery prima dello switch definitivo.

[Contratto](../design/F1_D05C_RC47_MIGRAZIONE_COPY_ON_WRITE.md) · [Piano test](F1_D05C_RC47_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md).
