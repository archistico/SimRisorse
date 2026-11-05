> **Aggiornamento RC49:** [ADR 0060](0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md) prevale sui requisiti di retrocompatibilità/migrazioni obbligatorie di database sperimentali. Questa ADR resta storica e le regole di integrità restano valide.

# ADR 0057 — Persistenza v15 alimentare isolata

**Data:** 2026-11-04 · **Stato:** implementazione F1-D05b candidata, gate Windows aperto.

## Contesto

Il v14 contiene Cibo, Pesce e Pane nello stesso inventario cittadino, mentre F1 prevede un deposito con scorte fisiche grezze e Cibo pronto. In-place conversion renderebbe possibile la doppia contabilizzazione o la perdita di una partita; il v14 è già il formato reale utilizzato dal giocatore.

## Decisione

Definire un archivio SQLite **v15 separato** (`user_version=4`, `civil-growth-15`) con le 11 tabelle di stato v14 ancora necessarie per la continuità della colonia, più due tabelle `STRICT` dedicate: `food_raw_stock`, `food_ready_stock`. Lo stock cittadino non può possedere i prodotti alimentari. Validazione del record combinato prima del salvataggio e dopo la lettura.

Non cambiare la sessione v14 né la sua persistenza. Il writer crea un file temporaneo verificato, salva un backup consistente prima di aggiornare un v15 precedente e rifiuta file di altre versioni nello stesso percorso. Non applicare una capacità arbitraria alle scorte importate, perché una vecchia partita può essere temporaneamente sovracapiente nel nuovo deposito.

## Conseguenze

- Nessuna migrazione automatica in D05b; D05c dovrà introdurre copia, controllo di somme, idempotenza e rollback.
- Il catalogo aggiuntivo e i nuovi edifici richiederanno estensione dello snapshot e dello schema in D05d prima dell'attivazione del gameplay.
- Il record degli stati produttivi ancora usa la struttura v14 come involucro interno di compatibilità; il file SQLite resta versione v15 autonoma.
- La politica di retention dei backup rimane non approvata: nessuna cancellazione automatica.
- Le decisioni E4b su Zecca, prezzi e squadre minime restano separate e non implementate.

[Contratto tecnico](../design/F1_D05B_RC46_PERSISTENZA_SQLITE_V15.md) · [Roadmap](../ROADMAP.md).
