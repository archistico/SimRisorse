# Piano di test — RC71 / F1-D05e.2b.2b.2f

**Stato:** implementati quattro nuovi test, non eseguiti con .NET 10. Ultima RC confermata: RC63, 771/771. RC70: 818 attesi, non confermati. **RC71: 822 test attesi.**

## Copertura nuova

| Test | Invariante |
| --- | --- |
| `CombinedClockRunsWithoutRetiredFamilyInNativeCity` | 15 produttori attivi senza slot, ricette reali e bilancio fisico coerente |
| `ExplicitlyImportedOccupiedRetiredProducerIsPreservedButNeverRuns` | Nessuna doppia produzione, nessuna perdita di lavoratori, equivalenza del secondo |
| `SuppressionPreservesFractionalAndSplitTickDeterminism` | Tick continuo/spezzato equivalenti, nessuno slot aggiunto |
| `OmittingAnActiveProducerIsStillRejectedByV15Validation` | La soppressione non indebolisce il vincolo sulle 15 famiglie attive |

## Sequenza quando l'utente potrà testare

1. ` .\validate.cmd ` (comando Windows effettivo ` .\validate.cmd ` senza spazi) e confermare 822/822, zero skipped; salvare i TRX. Se fallisce, bloccare RC72 e correggere.
2. ` .\run-game.cmd ` e ` .\run-game.cmd --v14 `: confronto visivo dell'ADR 0071 (blocchi grigi consultabili, dettaglio iconico, pulsanti, crescita, risorse a sette colonne, Scuola/Esercito).
3. `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined`: zero squilibri; osservare pasti serviti/mancati e input/output reali.
4. Dopo i test ordinari, audit combinato 24h (facoltativo e separato da `validate.cmd`).
5. Verificare round-trip SQLite generation 6, nessun `food-store` persistito, nessun accesso al salvataggio v14.

I controlli statici di questa consegna **non equivalgono** a compilazione, esecuzione della suite o ispezione visiva in Windows.

[Specifica](../design/F1_D05E2B2B2F_RC71_PLANNER_SENZA_SLOT.md) · [Roadmap](../ROADMAP.md) · [ADR 0079](../adr/0079-planner-v15-senza-slot-produttore-ritirato.md).
