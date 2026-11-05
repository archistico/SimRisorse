# ADR 0079 — Planner v15 senza edificio fittizio `food-store`

**Data:** 05/11/2026 · **Stato:** implementato in RC71, test .NET 10 pendenti · **Milestone:** F1-D05e.2b.2b.2f

## Problema

Il catalogo storico contiene `food-store`, che trasforma Pesce in Cibo nella v14. La v15 ha un solo proprietario del Cibo pronto: il nuovo Deposito alimentare. Anche dopo la rimozione del produttore dai checkpoint, `V15UnifiedCampaignClock` aggiungeva `BuildingState(food-store, 0, 0)` soltanto perché `ProductionPlanner.Plan` cercava una riga per **tutte** le definizioni del catalogo, incluse quelle disabilitate.

## Decisione

1. `ProductionPlanner.Plan` deve trattare una definizione inclusa nel parametro **esplicito** `suppressedProducerIds` prima di accedere al dizionario degli edifici. Se il relativo `BuildingState` manca, non crearne uno: produrre un report `NotProducer | NotConstructed | NoWorkers`, senza flussi, consumi o valutazione di input/capienza. Conservare l'eventuale numero di lavoratori riportato se una struttura importata esiste ancora; `RunnableWorkers` rimane zero.
2. Le definizioni **non** disabilitate restano obbligatorie: il planner continua a cercarle nel dizionario, senza indebolire le garanzie del motore v14.
3. Il clock v15 rimuove `TryAdd(food-store, 0, 0)`; non modifica prezzi, ricette, schedulazione, priorità, portafogli, formati SQLite o UI. La voce di catalogo storica rimane disponibile alla v14 per compatibilità del motore esistente; **non** è acquistabile in v15.
4. Conservare la cronologia acquisti del produttore ritirato e le eventuali importazioni occupate fino a un ritiro esplicito. Nessuna perdita silenziosa di lavoratori. Un producer ritirato, anche se occupato, non può produrre Cibo in v15.

## Verifica, rischi e confini

Quattro nuovi test Application: tick combinato senza slot, import storico occupato ma soppresso, equivalenza tick frazionari/spezzati, rifiuto di famiglia attiva mancante. Previsti **822 test cumulativi** (RC70: 818). I test .NET 10 **non sono stati eseguiti**. Rimangono da testare regressioni complete v14/v15, UI, persistenza e stress prolungato nel gate RC72.

Un `suppressedProducerIds` usato erroneamente da altri chiamanti potrebbe mascherare un produttore. È dunque un parametro volontario, e il default `null` mantiene invariata la verifica di completezza dello stato. In v15 l'elenco contiene esclusivamente `food-store`.

[Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md) · [Piano dei test](../validation/F1_D05E2B2B2F_RC71_TEST_PLAN.md) · [ADR 0078](0078-verifica-semantica-salvataggio-v15.md).
