# ADR 0016 — Zecca, Monete e conversione dei salvataggi v8

- **Data:** 2026-11-02
- **Stato:** Accettato e verificato su Windows
- **Ambito:** M5.2b, catalogo Application, Desktop, conversioni S1

## Contesto

Il contratto A di Forgia/Zecca è approvato dall'utente dal 28/10. Forgia/Spade è stata verificata in v8 con 235 test; v38 ha uniformato gli effetti grafici. L'[ADR 0015](0015-forge-swords-and-v7-save-upgrade.md) richiede un nuovo ID di regole per aggiungere la Zecca dopo il rilascio della Forgia.

## Decisione

- Introdurre **Zecca/Monete in `civil-growth-9`**, 19 edifici / 16 risorse. Zecca costa 210 Legno lavorato/160 Pietra, richiede Fonderia d'oro posseduta e un addetto; **1 Oro +1 Carbone → 3 Monete/s**, priorità 101 dopo Ferro 90, Oro 91 e Forgia 100.
- Usare pianificatore, precisione e capacità esistenti. Monete iniziali zero, capacità 1.000 +500 per Magazzino, lotti indivisibili. Con 1–2 posti residui nessuna produzione o spesa di input. Nessuna nuova semantica monetaria generale.
- Aggiungere la conversione **v8→v9**, concatenabile da v1: Zecca a quantità/addetti zero e Monete zero. Preservare Spade, Forgia/personale, scorte, persone, Magazzini, tempo e frazioni. Rifiutare vecchi checkpoint con Monete positive o righe Zecca incompatibili.
- Conservare **SQLite/snapshot v2**, slot civile e backup originale prima della prima scrittura. Nessuna produzione offline.
- Fissare il catalogo v8 con `CreateForgeForBenchmarks()` per le verifiche di compatibilità. V4/v5/v6/v7 rimangono invariati; M5.1c e l'esperimento Forgia/Zecca restano sul catalogo v7.
- M5.2b comprende primo lotto, capienza, priorità, persistenza e presentazione. **M5.2c resta un incremento separato**, con confronto delle filiere nel gioco corrente.

## Alternative

- Aggiungere Zecca a v8: respinto perché cambierebbe il significato di una versione già salvabile.
- Nuovo slot campagna: S2 già non scelta; continua la politica S1.
- Adattare la produzione a un lotto parziale per saturare 1.000 Monete: incompatibile col contratto approvato dei lotti interi. Da zero la capacità base consente 999 Monete.
- Usare Monete per tutti gli edifici: fuori dal ruolo approvato dei pagamenti espliciti futuri.

## Verifica e conseguenze

`validate.cmd` completo Windows: **258/258 test**, zero avvisi/errori, 20 cronologie più crescita accelerata, cinque benchmark, 33 PNG e smoke dei due scenari. Conversione/backup v8→v9 e round trip frazionario verificati su SQLite reale. Quattro render Zecca/Risorse ispezionati; [report M5.2b](../validation/M5_2B_MINT.md).

Quantità di Spade per recluta e Monete per nave restano da decidere nelle fasi M5.3/M5.4. Non sono introdotte navi, soldati o transazioni di pagamento.
