# ADR 0015 — Forgia, Spade e conversione dei salvataggi v7

- **Data:** 2026-11-02
- **Stato:** Accettato e verificato su Windows
- **Ambito:** M5.2a, catalogo Application, Desktop e continuità S1

## Contesto

L'utente ha approvato l'intero contratto A Forgia/Zecca il 28/10. Il [dossier M5.2](../design/M5_2_FORGE_MINT_PREFEASIBILITY.md) prevede incrementi distinti, verificando Forgia prima di Zecca. La baseline v36 e M5.1c sono state verificate con 211/211 test prima dell'incremento.

## Decisione

- Introdurre **Forgia e Spade in `civil-growth-8`**, 18 edifici / 15 risorse. Forgia costa 240 Legno lavorato / 180 Pietra, richiede una Fonderia di ferro posseduta, offre un posto e trasforma 1 Ferro +1 Carbone in 2 Spade/s, priorità 100 dopo le fonderie.
- Usare pianificatore, capacità, precisione e comandi esistenti. Scorta iniziale zero, capacità base 1.000 e +500 per Magazzino. Nessun output parziale per riempire un residuo insufficiente, nessun materiale consumato se il lotto non entra.
- Spade come normale scorta per il futuro arruolamento: nessun soldato o prezzo di reclutamento introdotto.
- Aggiungere la conversione **v7→v8**, concatenabile da v1. Nuova struttura a quantità/addetti zero; Spade a zero. Rifiutare scorte Spade positive o righe Forgia incompatibili prima dell'introduzione. Conservare scorte precedenti, popolazione, lavoratori, Magazzini, tempo e frazioni.
- Mantenere SQLite/snapshot **v2**, slot civile e backup alla prima scrittura secondo S1. Nessuna produzione offline.
- Conservare un catalogo v7 esplicito (`CreateGoldForBenchmarks`) per M5.1c e per il confronto sperimentale Forgia/Zecca, oltre ai cataloghi v4/v5/v6 già mantenuti.
- La Zecca richiederà **`civil-growth-9` e conversione v8→v9**. Non aggiungere nuovi contenuti a v8 dopo che può essere salvata.

## Alternative

- Forgia e Zecca insieme in v8: prevista dal dossier come alternativa tecnica, ma non adottata per mantenere incrementi verificabili separatamente.
- Nuovo slot di campagna: S2 già non scelta dall'utente; si conserva S1.
- Modifica al Core per gestire le Spade: inutile, perché ricette, prenotazioni, lotti e capacità sono già rappresentati dal motore corrente.

## Verifica e conseguenze

Gate completo Windows **235/235 test**, zero avvisi/errori, 18 cronologie più crescita accelerata, cinque benchmark e smoke dei due scenari. Conversione/backup su SQLite reale e round trip frazionario verificati. Quattro nuovi render ispezionati; [report M5.2a](../validation/M5_2A_FORGE.md).

Zecca, militari e navigazione restano incrementi successivi. Il contratto A della Zecca è già approvato; quantità di reclutamento e prezzi navali richiedono scelte nelle rispettive fasi.
