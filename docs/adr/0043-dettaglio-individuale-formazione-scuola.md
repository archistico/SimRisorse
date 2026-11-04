# ADR 0043 — Visualizzazione dei progressi individuali nella Scuola

- **Data:** 2026-11-04
- **Stato:** proposto, in attesa dei test RC32 su Windows
- **Milestone:** P1-D46

## Decisione

La Scuola espone un riepilogo individuale **di sola lettura** degli studenti con posto occupato. La prima barra (prossimo completamento) resta presente e i comandi rimangono dove sono. Mostrare fino a sei progressi e un riepilogo del resto riduce l'ingombro. I valori provengono direttamente da `SchoolResearchState.TrainingProgressSeconds` e vengono rigenerati sui normali refresh.

## Motivo

Una barra riferita solo al primo allievo non rende trasparente la formazione parallela in piu' Scuole. Non occorre introdurre nuove tabelle persistite, code aggiuntive, timers o regole economiche.

## Conseguenze

Nessun cambiamento ai salvataggi, al numero degli allievi, all'ordine di graduazione, alle Matite, ai costi o al tasso di ricerca. La v13 rimane intatta. Test di binding e invarianti della coda completano la copertura; e' richiesta validazione Windows.

[Rapporto D46](../design/P1_D46_RC32_CICLO_FORMAZIONE_SCUOLA.md) · [Piano test](../validation/P1_D46_RC32_TEST_PLAN.md)
