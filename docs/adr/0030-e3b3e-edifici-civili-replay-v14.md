# ADR 0030 — Costruzioni civili e replay della sessione v14

**Stato:** Accettato come scelta tecnica nel prototipo isolato; coefficienti economici NON ratificati. **Data:** 03/11/2026.

## Contesto

La v14 sperimentale possiede comandi per produttori e Scuola, ma Case, Magazzini e Caserme erano ancora immobili dopo la migrazione. Senza questi edifici non è possibile verificare una progressione completa e il replay di una partita reale con salvataggi intermedi.

## Decisione

Introdurre un comando di costruzione civile isolato che riutilizzi contatori e prezzi E1, verifichi prima i prerequisiti e si concluda con un unico commit validato. Conservare invariati snapshot v14 e schema SQLite v3. Validare un replay mediante due sequenze di comandi equivalenti, una delle quali attraversa un vero save/restore SQLite v3 e riprende esplicitamente dopo il caricamento in pausa.

## Conseguenze

Nessuna modifica a `GameSession` v13, SQLite v2, cataloghi giocabili o UI. I 4 tipi civili vengono costruiti nel prototipo, mentre il Municipio rimane immutabile. Le prove su prezzi e saldo ricerca non costituiscono approvazione dei numeri della futura campagna. Nessun event log persistente introdotto in questa fase.

[Contratto](../design/G1_D15_E3B3E_REPLAY_CIVILI_V14.md) · [Piano test](../validation/V60_E3B3_RC5_TEST_PLAN.md).
