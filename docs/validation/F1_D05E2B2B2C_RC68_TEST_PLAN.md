# RC68 — Piano validazione .NET 10

**Stato:** da eseguire su Windows; RC63 ultimo checkpoint confermato con 771/771. Le versioni successive non sono ancora state testate dall'utente.

## Validazione completa

```powershell
.\validate.cmd
```

**Conteggio atteso:** 806 = 797 RC67 + 9 RC68. Non considerare riuscita la RC68 in assenza dei risultati.

## Casi aggiunti

| Test | Aspettativa |
| --- | --- |
| `NativeCityUsesV15OwnedTypesNotV14FamilyOrCounter` | Nessuna collezione di DTO v14 nella proiezione civica |
| `ConversionPreservesEveryTierOccupancyIncludingWorkerAssignments` | Base/Grande/Avanzato e personale conservati |
| `PurchaseHistoryDoesNotResetWhenOwnedBuildingsDiffer` | Storia distinta dagli edifici attualmente posseduti |
| `ConvertedArraysRetainOriginalOrderAndIdentifiers` | Nessun riordino implicito |
| `RetiredHistoricalCounterIsPreservedWithoutRecreatingRetiredProducer` | Cronologia `food-store` sì, produttore attivo no |
| `BoundaryRejectsUninitializedLegacyCollections` | Conversione rifiuta collezioni non inizializzate |
| `BoundaryRejectsUninitializedNativeCollections` | Conversione inversa rifiuta collezioni non inizializzate |
| `NativeValidatorRejectsDuplicateProducerIdWithoutModifyingSource` | Controllo duplicati e immutabilità |
| `NativeValidatorRejectsRolledBackAcquisitionHistory` | Cronologia coerente con gli acquisti |

Due test RC67 di equivalenza sono stati adeguati per confrontare *valori* dei record, evitando il confronto dell'identità degli array `ImmutableArray` convertiti. Gli altri test rimangono invariati.

## Verifiche facoltative

```powershell
.\run-game.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined
```

La GUI deve restare identica alla RC63 validata e mostrare le sei filiere, edifici bloccati attenuati, elenchi con icone, pulsanti e barra di crescita storici. Se fallisce la compilazione, correggere prima di procedere alla RC69.

[Specifica](../design/F1_D05E2B2B2C_RC68_PORTAFOGLI_CONTATORI_V15.md) · [ADR 0076](../adr/0076-portafogli-contatori-v15-nativi.md).
