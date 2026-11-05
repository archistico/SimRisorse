# RC60 — Analisi del fallimento del budget occupazionale combinato

**Data:** 5 novembre 2026. **Contesto:** F1-D05e.2a; base confermata RC58 con 744 xUnit verdi. Il 05/11/2026 la RC59 ha compilato e ha eseguito 755 test: 754 passati, uno fallito nel test `FixtureHasEveryNewSiteStaffedWithoutExceedingTheSharedPopulationBudget` (atteso `AvailableWorkers=5`, reale `2`). Le suite Desktop e Infrastructure sono riuscite.

## Diagnosi dal codice

`EconomyFoundationScenario.Create()` crea tre edifici produttivi con un dipendente già assegnato per ciascuno: `tree-feller`, `sawmill`, `stonecutter`. La fixture sintetica `V15CombinedFoodStressScenario.Create()` usa `GameSession.Create(baseline).Snapshot()` per migrare il gioco in v14; **non azzera** il personale iniziale. In seguito sovrascrive solo le cinque famiglie alimentari, che impiegano tredici lavoratori complessivi, e crea sei strutture v15 con un addetto ciascuna.

| Gruppo | Impiegati |
| --- | ---: |
| Fondazione iniziale: Taglialegna, Segheria, Tagliapietra | 3 |
| Filiere storiche alimentari: Pescatore 1, Fattoria 3, Pozzo 7, Mulino 1, Forno 1 | 13 |
| Sei strutture v15 | 6 |
| **Occupati complessivi** | **22** |
| Popolazione iniziale sintetica | 24 |
| **Disponibili** | **2** |

`V15FoodIndustryCampaignSession.AvailableWorkers` sottrae correttamente dal totale abitanti tutti i lavoratori cittadini, le riserve scolastiche/militari e il personale alimentare v15. Il modello **non** presenta il difetto dedotto originariamente dal test: la fixture era stata descritta e assertita con un totale parziale.

## Modifiche minime

1. Corretto `Assert.Equal(5L, ...)` in `Assert.Equal(2L, ...)` nel test esistente.
2. Aggiunto il test `FixtureRetainsThreeFoundationWorkersWhenComputingCombinedFreePopulation`: verifica separatamente 3 addetti della fondazione, 13 della filiera storica, 16 cittadini, 6 nuovi, assenza di altre riserve e bilancio completo della popolazione.
3. Aggiornato soltanto il commento esplicativo della fixture (il codice produttivo non è cambiato), i numeri del rapporto RC59, il piano test e l'ADR 0068.

## Limiti e decisione

Nessuna modifica di gioco, valori economici, clock, tipologie di edifici, GUI, SQLite o salvataggi. Non azzerare i lavoratori iniziali per accontentare un'asserzione: sarebbe un cambiamento del comportamento del benchmark. Non considerare la RC60 validata prima dell'esecuzione reale della suite su Windows. Il precedente audit di 24h è valido **solo per il profilo base**; l'audit combinato non risulta ancora eseguito dall'utente.

**Prossimo:** dopo `756/756` e stress combinato breve, riprendere F1-D05e.2b e rimuovere strutturalmente `food-store` dal modello di checkpoint v15, separandolo definitivamente da v14.

[Piano RC60](../validation/F1_D05E2A_RC60_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md) · [ADR 0068](../adr/0068-stress-combinato-filiere-e-checkpoint-legacy.md).
