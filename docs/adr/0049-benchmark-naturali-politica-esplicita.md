# ADR 0049 — Benchmark economici naturali con politica deterministica esplicita

**Data:** 2026-11-04 · **Stato:** accettata per la strumentazione, gate runtime RC39 aperto.

## Contesto

E4A-06/07 confrontano Scuole con stock finanziato e non dimostrano la raggiungibilità dalle condizioni di partenza. E4a richiede invece tempi e motivi di blocco ottenuti da una campagna senza modifiche artificiali.

## Decisione

Introdurre una sola politica iniziale `natural-reference-v1` ripetibile per E4A-01…05, con snapshot iniziale generato dal gioco, azioni pubbliche v14, campioni ogni 60 secondi, log di azioni riuscite/rifiutate e limiti espliciti. Gli scenari differiscono per obiettivo e orizzonte, non rappresentano ancora cinque strategie indipendenti. Nessuna aggiunta artificiale di abitanti, risorse o ricerca, nessun salvataggio reale, nessuna sostituzione dei risultati mancanti con tempi teorici. Esportare JSON e CSV soltanto quando il runner viene eseguito e non sovrascrivere file esistenti.

## Conseguenze

Un timeout può indicare sia uno stallo economico sia una policy non ottimale: distinguerli in E4a-D03/D04. Conservare i risultati `not-run` finché l'utente non esegue il runner su Windows. E4b potrà iniziare solo dopo dati e approvazione esplicita dei valori proposti.

[Rapporto](../design/E4A_D02_RC39_BENCHMARK_NATURALI.md) · [Piano](../validation/E4A_D02_RC39_TEST_PLAN.md)
