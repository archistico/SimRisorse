# Rapporto G1-D29 / RC13 — Roadmap e primo audit GUI

## Avanzamento

La **RC12 è stata dichiarata validata automaticamente dall'utente** (468 casi attesi). RC13 parte da quella baseline, non implementa E4 e non cambia il disegno delle viste. La nuova [Roadmap](../ROADMAP.md) divide il lavoro in sei fasi con gate e rende l'audit G1-D29 il primo passo eseguibile.

## Difetto riprodotto per ispezione

`V14PreviewViewModel.Parity.cs`, ramo Matite di `CapacityBreakdown`: il testo conteneva contributi **statici** di Municipio/Magazzino/Scuola anche se tali edifici non erano posseduti. Inoltre mancavano saldo e spazio libero nel tooltip della nuova risorsa, a differenza di quelle della v13.

### Correzione RC13

La capacità delle Matite rimane calcolata dal Core (`V14SchoolSupplies.Capacity`), **senza cambiarne valori o ricetta**. La proiezione Desktop ora descrive solo gli edifici posseduti, con coefficienti rilevati dalle definizioni, e lo spazio libero. Il contributo della Scuola deriva dalla differenza tra la capacità effettiva del Core e quella già attribuita agli edifici di magazzino: niente secondo `+50` hardcoded da mantenere sincronizzato.

**Test aggiunti (2):** caso iniziale con solo Municipio; caso ripristinato 1 Municipio + 2 Magazzini + 1 Scuola con 10 Matite e selezione conservata dopo caricamento. Nessun cambiamento a v13, schema database, simulazione o XAML.

## Questioni aperte

- Suite Windows RC13 **non eseguita** qui (470 test attesi).
- Audit screenshot/hover 1040×740 e 1240×850 **aperto** su tutte le UI-01…UI-22.
- Coefficienti Matite/Scuola e prezzi E4 restano candidati, non bilanciati.

[Audit](../validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Piano RC13](../validation/G1_D29_RC13_TEST_PLAN.md) · [ADR 0038](../adr/0038-roadmap-gate-gui-e-audit-matite.md).
