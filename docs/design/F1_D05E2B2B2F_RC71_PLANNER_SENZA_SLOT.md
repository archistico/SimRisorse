# RC71 — Contratto del planner produttivo senza slot legacy

**Incremento:** F1-D05e.2b.2b.2f · **Base:** RC70 candidata · **GUI:** esattamente quella approvata nella RC63

## Comportamento prima/dopo

| Caso | Prima RC71 | Dopo RC71 |
| --- | --- | --- |
| v15 senza `food-store` nel checkpoint | Slot effimero introdotto nel clock | Nessuno slot; il planner salta la definizione disabilitata |
| v15 con vecchia struttura occupata importata | Definizione soppresa, stato conservato | Stessa soppressione e conservazione |
| v14 / pianificazione senza soppressione | Tutte le righe obbligatorie | Invariato |
| Report produttivo | Include `food-store` con `RunnableWorkers=0` | Stesso report, `RunnableWorkers=0` |
| Pesce → Cibo vecchio | Disabilitato | Disabilitato |
| Cibo nuovo dal Deposito | Abilitato | Abilitato |

### Invarianti

- `V15UnifiedCampaignClock` non inserisce `food-store` nel dizionario delle costruzioni.
- `ProductionPlanner` mantiene ordine di allocazione, richiesta delle righe attive e report diagnostici.
- Ogni ricetta **attiva** conserva input/output atomici nel secondo di simulazione. Il Deposito resta unico produttore di Cibo pronto v15.
- Nessun cambiamento a persistenza SQLite generation 6, GUI, prezzi, popolazione, ricerca o militari.
- La scomparsa del vecchio adattatore dal *planner* non autorizza la cancellazione automatica delle importazioni con edifici o lavoratori residui.

### Confine residuo

Il tipo `V15LegacyProducerAdapter` resta per le conversioni diagnostiche/importazione e per un ritiro esplicito. Il catalogo v14 continua a comprendere la propria ricetta. Eliminare entrambi sarebbe una modifica separata, non richiesta dalla RC71.

[ADR 0079](../adr/0079-planner-v15-senza-slot-produttore-ritirato.md) · [Test](../validation/F1_D05E2B2B2F_RC71_TEST_PLAN.md).
