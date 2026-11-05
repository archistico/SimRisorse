# F1-D05b — Test plan RC46

**Stato:** 600 test RC45 confermati, **9 nuovi test** in RC46, totale **609 attesi**. La compilazione/xUnit RC46 non è eseguita in questo ambiente.

## Gate automatici

Eseguire dalla root estratta su Windows con .NET 10:

```powershell
.\validate.cmd
```

Se il gate fallisce, **non** attivare la migrazione, non aggiornare il launcher e non sostituire alcuna partita. I nove test nuovi sono in `tests/SimRisorse.Infrastructure.Tests/V15FoodSaveStoreTests.cs`:

| Test | Vincolo |
| --- | --- |
| `V15RoundTripPreservesCityDepotAndMilliunitsExactly` | ResourceBundle cittadino, depo grezzi/pronti, Pane e frazioni invariati; anteprima v14 ricostruibile |
| `ANewV15FileDoesNotReadOrModifyTheV14Original` | Sorgente v14 identico a livello di byte e nessun backup v14 aggiuntivo |
| `V15CannotReplaceAnExistingV14FileEvenAtTheSameDestination` | Rifiuto con `UnsupportedFormat`, nessuna sovrascrittura |
| `OverwritingValidV15PreservesAPreviousVerifiedSqliteBackup` | Sostituzione con backup valido, leggibile e corrispondente al vecchio stato |
| `MissingReadyFoodRowIsRejectedAndExistingFileCannotBeOverwritten` | Stato incompleto rifiutato, checkpoint non modificato |
| `FoodCannotAppearInBothCityAndDepot` | Nessuna doppia contabilizzazione dopo scrittura o manomissione |
| `FutureFileVersionsAreRejectedWithoutMutatingTheFile` | Versioni future rifiutate, nessuna sovrascrittura |
| `FutureFoodAndHistoricalOverCapacityAreStoredWithoutLoss` | Carne/Frutta/Verdura e capienza storica preservate al millesimo |
| `MissingV15FileIsNotCreatedByLoad` | Nessuna creazione file durante il caricamento fallito |

## Prove statiche effettuate nella preparazione RC46

- Parsing del DDL v15 con SQLite locale: 13 tabelle `STRICT`, incluse `food_raw_stock` e `food_ready_stock`.
- Confronto RC45-DOC01 → RC46: solo nuovo Core, nuovo store Infrastructure, nuovi test e documenti.
- Link Markdown e archivio ZIP: controlli finali riportati al momento della consegna.

## Manuale dopo il gate

Non esiste ancora un'opzione nell'interfaccia per creare/caricare una partita v15. Avviare **normalmente** `run-game.cmd` deve utilizzare ancora il checkpoint v14 e visualizzare la partita senza modifiche. **Non rinominare né spostare** il database v14 per effettuare questa verifica.
