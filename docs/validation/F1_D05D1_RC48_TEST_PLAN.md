# RC48 / F1-D05d.1 — Piano test Windows e regressioni

**Input:** RC47 validata dall'utente con **620 test passati**. **Incremento:** 11 nuovi metodi xUnit in `tests/SimRisorse.Core.Tests/FoodIndustryTickPlannerTests.cs`. **Totale atteso:** **631 test**; nessuna suite .NET 10 eseguita localmente in RC48.

## Gate obbligatorio su Windows

Dopo estrazione dell'intero ZIP in una cartella non sovrapposta all'attuale progetto:

```powershell
.\validate.cmd
```

**Atteso:** 631 test passati, 0 falliti (a parità di discovery e di SDK, salvo test preesistenti dinamici). Non attivare la partita v15 sulla base di questo risultato: il formato live non è stato implementato.

## Nuovi test

| Nome | Invariante |
| --- | --- |
| `AllFiveNewIndustriesReachTheDepotWithCorrectIntermediateDelay` | Filiera coordinata in più passi, animali nuovi mai macellati subito, prodotti pronti solo via Deposito |
| `AnimalsWithoutButcherDoNotBecomeMeatOrReadyFood` | Nessuna macellazione implicita o Cibo gratuito |
| `MeatAndCropsCannotBypassAnUnstaffedDepot` | Produzione intermedia ≠ razione disponibile |
| `WaterIsConsumedOnceAndStableHasPriorityOverCrops` | Competizione acqua senza doppio consumo |
| `RawDepotIsBoundedAndUnsoldTransitIsNotDiscarded` | Saturazione preserva scorte di transito |
| `HistoricalOvercapacitySurvivesAndBlocksFurtherDeliveries` | Eccedenze storiche v15 conservate |
| `AdvancedBreadAndReadyFoodAreDeductedAtomically` | Riserva Pane, razione unica e fallimento senza addebito |
| `WorkersCannotExceedFreePopulationOrBuildingWorkplaces` | Posti e popolazione occupata non superabili |
| `DuplicateOrMisownedFoodIsRejectedBeforeTick` | Proprietà esclusiva di risorse alimentari |
| `StorageOfNewIntermediateResourcesNeverEntersLegacyCity` | Intermedi non scrivibili impropriamente nella città v14/v15 |
| `PriorityAndResultsDoNotDependOnResourceBundleInsertionOrder` | Determinismo dei contenitori di risorse |

## Audit supplementare

- Diff sorgenti rispetto a RC47: **una classe Core e un file di test**, niente modifiche ai file C# già presenti.
- XML/XAML, JSON, link Markdown relativi, manifest ZIP e lettura integrale dell'archivio.
- Controllo manuale: l'avvio `run-game.cmd` deve continuare a usare v14, la GUI e il proprio DB storico senza alcuna migrazione automatica. `--legacy-v13` deve usare il suo percorso separato.

## Gate successivi, NON coperti da RC48

F1-D05d.2: round-trip dei nuovi stock, nuovi edifici e lavoratori, compatibilità v15, test di corruzione e backup. F1-D05d.3: crescita/ricerca/economia sullo stesso orologio, bilanciamento lavoratori e recupero post-crash. F1-D05d.4: UX sette colonne, edifici costruibili, strumenti di diagnostica e nuovi test desktop. F1-D05e: stress/saturazione/consolidamento. Questi test non devono essere considerati già superati.

[Dettagli](../design/F1_D05D1_RC48_COORDINAMENTO_FILIERA.md) · [ADR 0059](../adr/0059-confini-produzione-alimentare-e-proprieta-stock.md).
