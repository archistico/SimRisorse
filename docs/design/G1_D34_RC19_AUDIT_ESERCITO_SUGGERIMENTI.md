# G1-D34 / RC19 — Coerenza suggerimenti Esercito

**Data:** 03/11/2026. **Tipo:** correzione presentazione v14, nessuna nuova regola.

## Evidenza e problema UI-15/UI-20

Nella RC18 il `V14PreviewViewModel.Parity.cs` disabilitava già `EnlistCommand` e `CancelRecruitsCommand` mentre il giocatore confermava «Nuova partita?», ma le stringhe `MilitaryRecruitmentHint` e `MilitaryCancelHint` continuavano a essere calcolate solo su caserme, abitanti e Spade. Il risultato poteva annunciare **«Arruolamento possibile»** mentre il pulsante era in realtà disabilitato. Anche `MilitaryLimitsNote` dichiarava «Arruolabili ora» senza tener conto del blocco temporaneo.

Inoltre, per i lotti `+5` / `+10` il messaggio di errore «Spade insufficienti» oppure «Abitanti liberi insufficienti» non mostrava i valori richiesti/disponibili. «Costo per 1 reclute» aveva un plurale errato.

## Intervento

- `MilitaryRecruitmentHint`, `MilitaryCancelHint` e `MilitaryLimitsNote` segnalano esplicitamente la conferma di nuova partita in corso, per prima cosa e senza suggerire azioni disabilitate.
- I suggerimenti indicano **lotto selezionato**, **quantità disponibili** e **quantità necessarie** per Spade, abitanti liberi, reclute annullabili e spazio per il rimborso.
- `MilitaryWeaponsStatus` usa «recluta» al singolare.
- Nessun cambiamento ai predicati di `CanExecute`, ai tasti o alla disposizione della schermata, alla progressione individuale, ai rimborsi, al motore, alla GUI v13 o ai file SQLite.

## Test aggiunti

1. `MilitaryHintsExplainTemporaryNewGameBlockAndRestoreAfterCancel`: comando bloccato e messaggi coerenti durante conferma e dopo Annulla.
2. `MilitaryBatchHintsReportActualSwordAndRecruitsShortages`: checkpoint di prova con una Caserma e cinque Spade; lotti +1/+5, reclutamento e rimborso, numeri e stati dei comandi.
3. `MilitaryPanelUpdatesHintsAndEnabledStateWhenQuantityAndConfirmationChange` (Avalonia headless): aggiornamento del testo e del pulsante nella schermata Esercito dopo apertura/annullamento conferma e cambio lotto.

**Baseline Windows verde:** RC16, 476 test. **RC17:** 481 attesi non testati; **RC18:** 484 attesi non testati; **RC19:** 487 attesi (+3) non compilati/eseguiti nell'ambiente di preparazione. L'esito statico non certifica gli stili/hover delle due risoluzioni né la suite Windows. Il gate P0 UI-01…UI-22 resta aperto.

[Validazione RC19](../validation/G1_D34_RC19_TEST_PLAN.md) · [Audit GUI](../validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Roadmap](../ROADMAP.md).
