# RC58 — Piano di validazione dei confronti SQLite

**Base:** RC56 731/731 confermati; RC57 **741/743** con due errori di confronto per ordine; RC58 +1 test, **744 attesi**. I numeri non sono una prova di superamento della RC58.

## Esecuzione consigliata su Windows .NET 10

```powershell
dotnet test tests/SimRisorse.Infrastructure.Tests/SimRisorse.Infrastructure.Tests.csproj -c Release --filter FullyQualifiedName~F1D05e1V15StressPersistenceTests
.\validate.cmd
```

### Checkpoint da controllare

| Test | Obiettivo |
| --- | --- |
| `MultipleSqliteRestartsWithFractionalTicksMatchUninterruptedRun` | Quattro restart SQLite, frazioni temporali e equivalenza del clock ininterrotto, senza dipendere dall'ordine dei record per chiave |
| `SQLitePreservesCityAndDepotIngredientsWithDistinctIntermediateOwners` | Persistenza e round-trip di Pesce, Pane, Bovini/Selvaggina, Carne/Ortofrutta e Cibo pronto con proprietari distinti |
| `KeyedCampaignCollectionsCanBeReorderedWithoutChangingCheckpointMeaning` | L'inversione di quattro collezioni identificabili per ID non cambia lo stato logico confrontato |
| Resto della suite | Nessuna regressione nelle altre 741 verifiche RC57, nessuna modifica a Desktop/GUI |

**Atteso:** 744 test cumulativi superati; non cambiare il codice di produzione per accomodare il confronto. Se i test proseguono oltre la vecchia riga 77 e falliscono su un altro campo, acquisire il nuovo TRX e diagnosticarlo puntualmente.

Stress opzionale **solo dopo gate verde**:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 600
# Facoltativo: --audit-v15 --hours 24
```

Non segnare lo stress 24h come eseguito se non è stato lanciato esplicitamente.

[Rapporto](../design/F1_D05E1_RC58_FIX_ORDINE_CHECKPOINT.md) · [Roadmap](../ROADMAP.md).
