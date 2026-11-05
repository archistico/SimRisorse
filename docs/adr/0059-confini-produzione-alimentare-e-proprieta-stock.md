> **Aggiornamento RC49:** [ADR 0060](0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md) prevale sui requisiti di retrocompatibilità/migrazioni obbligatorie di database sperimentali. Questa ADR resta storica e le regole di integrità restano valide.

# ADR 0059 — Coordinamento delle filiere alimentari e proprietà esclusiva degli stock

**Data:** 5 novembre 2026. **Stato:** accettato come confine tecnico, implementazione Core isolata in RC48; attivazione gameplay differita.

## Problema

Il checkpoint SQLite RC47 ha le scorte alimentari grezze e pronte, ma non ha record per Bovini, Selvaggina, prodotti lavorati ancora fuori dal Deposito, né le nuove strutture e il relativo personale. Riutilizzare le colonne `resource_stock` della vecchia città violerebbe le validazioni di proprietà e potrebbe comportare perdita o doppia contabilizzazione.

## Decisione

- Coordinare Stalla, Cacciatore, Macellaio, Orto, Frutteto e Deposito in **un passo deterministico puro** con ordine esplicito; le quattro fasi sono produzione animale, ortofrutta, trasferimento/lavorazione nel deposito, razione.
- Separare gli inventari `City`, `Animals`, `Transit`, `Depot`; nessun ingrediente può appartenere a più inventari durante lo stesso checkpoint. I beni bloccati da saturazione non vengono scartati.
- Eseguire un unico preflight della forza lavoro prima del tick: assegnazioni per struttura, posti realmente disponibili e popolazione già impegnata; nessun addetto gratuito o prestito implicito fra filiere.
- Usare solo gli animali presenti all'inizio del passo per la macellazione; l'acqua viene assegnata prima alla Stalla, poi alle coltivazioni; il Pane riservato non è trasformato due volte.
- **Non** introdurre nuovi campi in un file ancora dichiarato `user_version=4` e `civil-growth-15`; ogni estensione incompatibile richiede versione nuova, store validante e migrazione copy-on-write. Senza questa garanzia non collegare il nuovo calcolo al clock della partita attiva.

## Conseguenze

RC48 aggiunge `FoodIndustryTickPlanner` e regressioni Core, ma non rende gli edifici costruibili e non cambia GUI, salvataggi né costi. In F1-D05d.2 si progetta e implementa lo snapshot e la persistenza estesa; D05d.3 aggancia costruzione, lavoratori, clock, backup/attivazione; D05d.4 integra GUI e test su avvio/caricamento. Seguono F1-D05e ed E4a.

**Alternative respinte:** usare una sola scorta cittadina per tutti i prodotti, ignorare i transiti per saturazione, lasciare che il Macellaio utilizzi animali appena generati, attivare il nuovo tick senza persistenza completa.

[Specifica RC48](../design/F1_D05D1_RC48_COORDINAMENTO_FILIERA.md) · [Validazione](../validation/F1_D05D1_RC48_TEST_PLAN.md) · [ADR 0058](0058-migrazione-v14-v15-senza-sovrascrittura.md).
