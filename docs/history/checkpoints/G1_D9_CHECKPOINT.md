# G1-D9 — SimRisorse E3b/2 (3 novembre 2026)

**Base verificata dall'utente:** v60-E3b-RC1: tutti i test passati su Windows. Il suo `civil-growth-13` rimane la campagna giocabile.

**Nuovo lavoro:** `V14SqliteSaveStore` sperimentale in Infrastructure (file distinto), schema SQLite STRICT v3, scrittura transazionale su file temporaneo, controllo integrale del risultato, backup S1 e sostituzione protetta; import da v13 solo in copia. Nuova suite `V14SqliteSaveStoreTests` con 11 casi. Il formato e il catalogo v13, `GameSession`, `SqliteGameSaveStore` e la GUI sono invariati.

**Verifiche locali:** DDL SQLite v3 validato con motore sqlite3 (11 tabelle), sorgenti/test precedenti confrontati byte per byte con la v60-E3b-RC1, struttura ZIP verificata. **Non effettuata compilazione .NET** nell'ambiente di lavoro.

**Prossimo:** `validate.cmd` su Windows (quando disponibile). Poi E3b/3: sessione v14, Scuola a catalogo, comandi di formazione, spesa ricerca e upgrade, senza attivare immediatamente gli incrementi di costo. Infine E3b/4 GUI, E4 bilanciamento e validazione reale.

[Dettaglio](../../design/G1_D9_E3B2_SQLITE_V3_ISOLATO.md) · [ADR 0025](../../adr/0025-e3b2-sqlite-v3-copy-on-write-isolato.md)
