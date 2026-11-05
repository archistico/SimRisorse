> **Aggiornamento RC49:** [ADR 0060](0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md) prevale sui requisiti di retrocompatibilità/migrazioni obbligatorie di database sperimentali. Questa ADR resta storica e le regole di integrità restano valide.

# ADR 0058 — Migrazione alimentare v14 → v15 copy-on-write

**Data:** 2026-11-05  
**Stato:** Accettato come strategia di isolamento per F1-D05c; implementazione candidata, da validare in Windows.

## Contesto

La nuova economia alimentare richiede scorte grezze e Cibo pronto nel Deposito, senza reinterpretare il database giocabile v14. In RC46 esiste un nuovo store v15 autonomo, ma manca il passaggio operativo che ne dimostri la conservazione dei dati.

## Decisione

- Mai sovrascrivere, rinominare o cancellare il salvataggio v14 durante la preparazione.
- Copiare le scorte tramite `V14FoodStockMigration`; controllare equivalenza al millesimo e gli altri stati della colonia; le eccedenze non devono essere troncate.
- Scrivere il v15 solo su staging nello stesso filesystem, verificarlo, quindi eseguire promozione esclusiva (`File.Move` senza overwrite). In caso di collisione: rifiuto.
- Seconda migrazione con contenuto identico: no-op idempotente; una v15 differente o evoluta: mai sostituita.
- Nessuno switch automatico della partita fino a F1-D05d. Il database v14 rimane il rollback; prima dello switch reale serviranno backup e recovery dedicati.
- Migrare soltanto con gioco chiuso; SHA del file principale e doppie letture non sono una garanzia transazionale per writer WAL contemporanei.

## Alternative respinte

1. **Aggiornamento in-place v14:** incompatibile con reversibilità e separazione dei ruleset.
2. **Overwrite di una v15 esistente con backup:** rischierebbe di eliminare progressi già compiuti nella nuova partita.
3. **Troncamento delle scorte sovracapienti:** perdita silenziosa di lavoro del giocatore.
4. **Commutazione automatica al primo avvio:** non gestibile prima della sessione v15 completa.

## Conseguenze

In RC47 viene aggiunto solo il migratore Infrastructure isolato con 11 test; nessuna modifica ai costi, alle squadre Zecca, al gameplay o ai database in uso. Il meccanismo di attivazione con backup e rollback definitivo rimane F1-D05d/e.

[Dettaglio](../design/F1_D05C_RC47_MIGRAZIONE_COPY_ON_WRITE.md) · [Test](../validation/F1_D05C_RC47_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
