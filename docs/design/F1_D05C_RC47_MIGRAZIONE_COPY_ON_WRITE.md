# F1-D05c / RC47 — Migrazione v14 → v15 copy-on-write

**Stato:** implementata come servizio isolato `V14ToV15FoodMigrator`; **RC47 candidata**, test Windows non eseguiti. **Ultimo gate positivo confermato:** RC45 600/600. L'operazione non è collegata al launcher, all'autosalvataggio o alla GUI.

## Contratto e responsabilità

- **Origine:** un checkpoint v14 SQLite v3 (`civil-growth-14`) valido e caricato in sola lettura mediante `V14SqliteSaveStore`.
- **Destinazione:** un *altro* percorso, per SQLite v4 / `civil-growth-15`, da aprire con `V15SqliteFoodSaveStore`.
- **Contenuto:** Cibo pronto nel deposito, Pesce/Pane/Carne/Frutta/Verdura come scorte alimentari grezze; tutte le altre risorse e lo stato della colonia invariati. **Nessuna conversione o razione automatica durante la migrazione.**
- **Sovracapacità:** `FoodMigrationCapacityCheck` indica gli scostamenti, ma i millesimi eccedenti vengono preservati. Nessun ribilanciamento automatico.
- **Preflight:** `Preview(rawCapacities, readyFoodCapacity)` legge la v14 e calcola il piano, senza creare file di destinazione, backup o scorte.
- **Esecuzione esplicita:** `MigrateCopy(rawCapacities, readyFoodCapacity)` è una API, **non un comando di gioco né una conversione automatica**.

## Ordine delle operazioni

1. Rifiutare identità di percorso fra v14 e v15; acquisire SHA-256 dei byte del file principale v14 (identità diagnostica, non backup).
2. Validare il v14 con lo store esistente, separare le scorte con `V14FoodStockMigration.Prepare`, verificare conservazione totale per ogni risorsa al millesimo, calcolare l'eventuale eccedenza di capacità.
3. Rileggere il checkpoint v14 e confrontare l'intero contenuto logico, non soltanto le scorte: popolazione, tempo, crescita, edifici, tier, acquisti, lavoratori, ricerca, scuola, progressi individuali, militari, tecnologie e penne/Matite pagate. Verificare nuovamente l'impronta del file principale.
4. Se esiste già la destinazione v15, caricarla e confrontarla con la migrazione proposta. Se è **identica**, restituire `AlreadyPresent=true` senza scrivere; se differisce oppure non è una v15 valida, rifiutare la migrazione e **non sovrascrivere nulla**.
5. Altrimenti generare una copia v15 in un percorso temporaneo nella directory della destinazione; usare lo store v15 (che scrive e valida un suo file temporaneo) e rileggere il checkpoint completo.
6. Verificare equivalenza logica del checkpoint v15 e ricostruibilità del v14 originario, incluse le scorte. Rileggere ancora il v14. **Solo a questo punto** eseguire `File.Move` senza opzione di sovrascrittura.
7. Rileggere il file v15 definitivo e restituire ricevuta (percorsi, SHA-256, deficit di capacità, indicatore `AlreadyPresent`). Il file v14 originale **non viene mai modificato o eliminato**.

## Rollback e limiti

- Rollback **operativo**: il gioco continua a utilizzare la vecchia v14. La copia v15 si può scartare esplicitamente senza perdere una singola scorta della vecchia partita. In RC47 **non esiste alcuno switch di partita attiva** da ripristinare.
- Non c'è una copia *aggiuntiva* del v14: **l'originale conservato è il checkpoint di rollback**, che per questa release non viene rinominato né sostituito. In F1-D05d, quando si introdurrà lo switch, va aggiunto un backup dedicato prima dell'attivazione.
- **Gioco chiuso obbligatoriamente per una migrazione operativa futura.** L'SHA-256 del file SQLite principale non copre autonomamente le transazioni in un WAL laterale. Le doppie riletture logiche riducono il rischio di deriva, ma non consentono di dichiarare sicura una migrazione simultanea con un writer attivo. Non usare come strumento di migrazione live.
- Una collisione di destinazione verificatasi fra la preparazione e `File.Move` fallisce senza sovrascrittura. Il file di staging è rimosso dopo gli errori, senza eliminare nessun salvataggio preesistente.
- Non c'è rollback *automatico* di un v15 definitivo se una successiva lettura fallisce per corruzione esterna o crash: il v14 originale resta intatto. Operazioni di attivazione, scelta del checkpoint e gestione del crash saranno gate separati in F1-D05d/e.
- Il v15 in RC47 mantiene ancora i 16 gruppi produttivi ereditati dal v14. Il catalogo e il tick reale delle cinque nuove filiere non sono attivi, e il formato v15 andrà esteso e versionato prima di diventare il salvataggio ordinario.

## API di esempio (uso esclusivo test/sviluppo)

```csharp
var migration = new V14ToV15FoodMigrator(v14Path, v15Path,
    scenarioV13, schoolRules, technologyCatalog, foodPolicy);
var preview = migration.Preview(rawDepotCapacities, readyFoodCapacity);
// Controllare preview.Capacity.Fits e le eventuali eccedenze.
var receipt = migration.MigrateCopy(rawDepotCapacities, readyFoodCapacity);
// receipt.AlreadyPresent distingue una seconda richiesta idempotente.
```

**Non modificare i percorsi reali dei database utente per eseguire questi esempi:** RC47 non espone una migrazione UI/CLI. Per la verifica usare i test isolati in cartelle temporanee.

[ADR 0058](../adr/0058-migrazione-v14-v15-senza-sovrascrittura.md) · [Piano test](../validation/F1_D05C_RC47_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
