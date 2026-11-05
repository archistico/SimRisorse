# F1-D05b — Contratto SQLite v15 separato (RC46)

**Stato:** codice candidato, da compilare e validare su Windows. **Baseline confermata:** RC45 / 600 test. Il gameplay rimane v14. Non è una migrazione automatica.

## Obiettivo e confine di fiducia

Il nuovo adattatore `V15SqliteFoodSaveStore` scrive in un **percorso nuovo** SQLite `user_version=4`, `rules_version=civil-growth-15`, `application_id` compatibile con SimRisorse. Il v14 attivo resta `user_version=3` e vive nella posizione storica. Nessuna finestra/servizio usa automaticamente il nuovo store in questa release.

Il checkpoint di transizione `V15FoodCampaignCheckpoint` contiene:

- `CityCampaign`: componenti civili/militari/ricerca e **solo le risorse non alimentari**, ancora espressi dai record v14 per compatibilità (format 3 / civil-growth-14 **interni**).
- `Depot.RawFood`: quantità di Pesce, Pane, Carne, Frutta, Verdura immagazzinate, senza duplicati nell'inventario cittadino.
- `Depot.ReadyFood`: Cibo pronto, già trasferito dall'inventario cittadino.

L'intestazione **del file esterno** è v15 (`snapshot_version=4`, `rules_version=civil-growth-15`); non è un database v14 rinominato. Le quantità sono `INTEGER milliunits >= 0`, senza conversioni a `float`. Il nuovo formato usa 13 tabelle `STRICT`: le 11 tabelle degli stati consolidati (colonna `resource_stock` per **sole scorte non alimentari**) più `food_raw_stock` e `food_ready_stock` (riga `slot=1` obbligatoria).

## Regole d'integrità

1. La politica alimentare deve descrivere esattamente Pesce, Pane, Carne, Frutta e Verdura; Pane e Cibo pronto sono identificatori distinti.
2. Scorte positive per un alimento sono ammesse **in un solo luogo**, il deposito. Scorte alimentari nell'inventario cittadino e risorse non alimentari nel deposito sono rifiutate.
3. Personale, popolazione, livelli, ricerca, addestramento e materiali non alimentari usano la validazione della campagna v14 consolidata. Non si cambia il gameplay per l'esistenza del nuovo formato.
4. Le quantità **sovracapienti storiche** non vengono troncate: la politica di rientro nella capacità sarà implementata in F1-D05c/d. Anche i nuovi ingredienti non presenti nel catalogo v14 possono essere persistiti nel deposito, senza reinterpretarli come scorte cittadine.
5. File estranei, v13/v14 nello stesso percorso, versioni future, tabelle inattese, righe obbligatorie mancanti, collezioni incoerenti e dati corrotti devono essere rifiutati. Non creare il file su `Load()` mancante.
6. Save in file temporaneo nella **stessa directory**, riapertura e verifica prima della promozione; se il file v15 esiste, lettura/validazione preventiva e backup SQLite consistente tramite `VACUUM INTO` prima della sostituzione. Backup storici mai sovrascritti, overflow oltre i 100 nomi numerati senza pulizia automatica.
7. I salvataggi v14 reali non vengono **mai** aperti in scrittura dal nuovo adattatore. Una preview `V14SqliteSaveStore.PreviewFoodMigration(policy)` è read-only; `FromMigrationPlan` compone solo oggetti in memoria. Scrivere in un percorso v15 è un'operazione esplicita di chiamante/test, non il passaggio automatico F1-D05c.

## Limitazioni intenzionali

- Il record `CityCampaign` riutilizza la struttura v14 e le sue 16 famiglie di produzione. Stalla, Macellaio, Cacciatore, Orto, Frutteto e relativi tier saranno aggiunti allo stato v15 durante **F1-D05d** con regole e test specifici, non forzati in una partita oggi.
- La mappatura inversa `ToLegacyPreview` è **diagnostica**: ricostruisce solo le risorse che il v14 conosce. Una colonia v15 evoluta non deve essere salvata di nuovo con il writer v14.
- Lo store non è ancora collegato a `run-game.cmd`, all'autosalvataggio o alla UI. F1-D05c gestirà migrazione copy-on-write, scelta esplicita del nuovo checkpoint, rollback e invarianti di conservazione.
- Il DDL dello stato condiviso è deliberatamente congelato al contratto già validato: prima dell'attivazione del catalogo v15 occorrerà estendere il modello delle famiglie con un'ulteriore revisione di schema.

## Sicurezza di migrazione richiesta per D05c

1. Leggere e validare v14 in sola lettura; acquisire impronta e metadati del file originale.
2. Calcolare una **singola** assegnazione delle scorte (`V14FoodStockMigration`); calcolo di equivalenza in millesimi per tutte le risorse originarie.
3. Generare il v15 in un file diverso, validarlo tramite nuovo lettore, confrontare popolazione, edifici, ricerca, militari, scorte e frazioni.
4. Solo dopo il controllo completo, rendere attivo il percorso v15 con un passaggio reversibile e testare interruzione e ripresa; conservare il v14 come rollback.
5. Nessuna conversione Pane→Cibo automatica durante la migrazione; i consumi iniziano solo nella nuova sessione e un Pane non può essere consumato due volte.

[ADR 0057](../adr/0057-sqlite-v15-separato-per-deposito-alimentare.md) · [Piano test RC46](../validation/F1_D05B_RC46_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
