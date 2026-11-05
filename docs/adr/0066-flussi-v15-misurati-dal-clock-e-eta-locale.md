# ADR 0066 — Flussi v15 misurati dal clock e stima locale di riempimento

- **Data:** 5 novembre 2026
- **Stato:** implementata nella RC56, validazione .NET 10 pendente
- **Contesto:** RC55 confermata dall'utente con **713 test passati**. La GUI v15 mostrava «—» nelle colonne Produzione, Consumo, Saldo e Si riempie tra perché non esisteva una contabilità lorda affidabile del clock alimentare.

## Decisioni

1. **Unica fonte:** `V15UnifiedCampaignClock.Plan` registra i flussi lordi *del primo secondo intero* direttamente presso gli stessi planner delle ricette storiche, Stalla/Cacciatore/Macellaio, Orto/Frutteto e Deposito. Il nuovo `V15EconomyProjector.Project` simula il prossimo secondo su una copia del checkpoint, senza avanzare la sessione. Non introdurre una seconda versione delle formule di produttività.
2. **Contabilità:** Produzione /s = quantità effettivamente generate nel secondo prossimo; Consumo /s = materie prime effettivamente consumate da ricette e razioni servite; Saldo /s = differenza. Il trasferimento fra proprietari fisici (città → Deposito, animali → transito, transito → Deposito) **non è produzione né consumo**. Il Pesce/Pane nel deposito e nella città si sommano senza duplicazione, e il Cibo pronto è posseduto soltanto dal Deposito.
3. **Limiti:** il valore indica **il prossimo secondo se lo stato è immutato fino a quel tick**; non è una previsione plurioraria, né una produzione teorica per lavoratore. Gli addetti senza input, la capienza piena, l'ordine delle ricette e il blocco della razione atomica possono produrre valori nulli anche in presenza di edifici.
4. **ETA:** se capacità nulla → «—»; se quantità già alla capacità o oltre → `00:00:00`; se saldo ≤ 0 → «—»; altrimenti `ceil((capacità − quantità) / saldo)`, formattato `hh:mm:ss` (ore non limitate a 24). Tooltip esplicito sulla natura della stima *a saldo costante*, non garantita.
5. **Capienza:** ogni riga mostra la somma dei contenitori fisici che la compongono; tooltip dettaglia Città/edifici, Recinti animali, Lavorazioni intermedie, Deposito ingredienti o Deposito Cibo pronto. Conservare sette colonne, ordine alfabetico, numeri a destra e quantità arrotondata all'unità.
6. **Isolamento:** la GUI v14, i suoi file XAML, il salvataggio v14 e la persistenza SQLite v15 `user_version=5` restano invariati. Nessuna modifica a ricette, prezzi o bilanciamento E4b.

## Criteri di accettazione

- Totale stock per risorsa dopo un tick = totale prima + produzione lorda − consumo lordo (al millesimo), anche attraversando i compartimenti.
- Cibo produce solo con Deposito dotato di personale; `food-store` storico non produce nel clock v15.
- Niente consumo di Cibo quando la razione combinata Cibo+Pane è bloccata; la produzione e il consumo sono registrati separatamente quando avvengono entrambi.
- `V15PreviewViewModel.Refresh()` non altera l'orologio, non sostituisce gli oggetti riga selezionati, né scrive SQLite.
- `validate.cmd` su Windows: **731 test previsti** (713 RC55 validati + 18 nuovi casi). Non dichiarare RC56 validata fino alla conferma.

[Specifiche RC56](../design/F1_D05D4B2_RC56_FLUSSI_ETA.md) · [Piano test](../validation/F1_D05D4B2_RC56_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md).
