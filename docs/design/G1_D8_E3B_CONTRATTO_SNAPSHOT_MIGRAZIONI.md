# G1-D8 — E3b/1: contratto di snapshot e conversione v13 → v14

**Data:** 2026-11-03  
**Stato:** implementazione Core isolata, **NON compilata né testata su .NET**; DDL SQLite proposto, **NON applicato**.  
**Baseline ultima validata su Windows:** v59-RC1 / `civil-growth-13`.  
**Ramo di sviluppo:** v60-E3b-RC1, costruito su E3-RC1 non ancora validata.

## Scopo dell'incremento

Definire prima dell'attivazione la **struttura completa dei dati** del nuovo sistema economico, evitando che i 16 produttori a livelli 1/3/10 vadano a corrompere il vecchio `BuildingState` (che consente un solo lavoratore per fabbricato).

Il nuovo modello `V14CampaignSnapshot` è separato dallo snapshot v13: non cambia `GameSnapshot`, `GameSession`, `SqliteGameSaveStore`, l'interfaccia Avalonia o il ruleset attivo. È un **contratto di stato futuro** con migrazione deterministica e controlli incrociati, non ancora un salvataggio utilizzato dalla partita.

## Modello dati proposto

- `FormatVersion=3`, `RulesVersion=civil-growth-14`, scenario `civil-growth`.
- Tempo (`ElapsedTicks`), abitanti, risorse, progresso della crescita e stato militare sono conservati.
- `OtherBuildings`: le strutture non produttive, incluso il Municipio fisso, restano nel formato aggregato esistente.
- `ProductiveFamilies`: esattamente le **16 famiglie** produttive, con tre coppie `(quantità, lavoratori)` per i livelli base/grande/avanzato.
- `PurchaseCounters`: ogni famiglia non fissa ha un contatore *monotono* almeno pari agli edifici posseduti; i miglioramenti di livello non incrementano il contatore. Anche `school` ha un contatore dedicato. Il Municipio è escluso.
- `SchoolCount`, `SchoolResearchState`: Scuole, studenti in attesa, progressi individuali, ricercatori qualificati e attivi. Gli attivi sono un sottoinsieme dei qualificati, tutti gli studenti e ricercatori occupano abitanti reali.
- `ResearchBalanceMilliPoints` in intero 64-bit, `UnlockedTechnologyIds` in lista esplicita. I punti possono accumularsi senza selezionare ricerche; nessuno sblocco automatico.

## Migrazione deterministica del solo checkpoint v13

`V14CampaignSnapshots.MigrateFromV13`:

1. Richiede uno snapshot **esattamente** `civil-growth-13`/formato 2, con scenario compatibile.
2. Passa prima dal `GameSession.Restore` esistente: vengono controllati stock, Municipio, popolazione, militari, edifici e capacità prima di trasformare i dati.
3. Converte gli edifici produttivi v13 in **livello Base**; zero Grandi e Avanzati, senza aggiungere lavoratori.
4. Converte gli edifici non produttivi senza variazioni.
5. Inizializza i contatori dagli edifici posseduti; Scuola `0`, punti ricerca `0`, nessuna tecnologia sbloccata e nessun ricercatore.
6. Esegue `V14CampaignSnapshots.Validate` sul risultato prima di restituirlo.

L'originale è immutabile. Il prototipo **non legge né scrive** database personali. Le versioni antecedenti la v13 seguiranno in futuro le migrazioni già presenti fino a v13, quindi questa conversione.

## Controlli sul nuovo stato

- Insieme completo e senza duplicati delle 16 famiglie e delle strutture ordinarie; identificativi sconosciuti rifiutati.
- Posti massimi per livello: 1, 3, 10. Nessun edificio avanzato senza ricerca acquistata (e prerequisiti coerenti).
- Tutti gli abitanti riservati a lavori, caserme, formazione e ricerca sono contati **una volta sola**; non possono superare la popolazione.
- Il Municipio rimane fisso e unico. Alloggi e progressione della popolazione mantengono i vincoli v13.
- Stoccaggio ricalcolato sui posti equivalenti degli edifici produttivi (1, 3, 10) e sullo stoccaggio degli edifici ordinari; lo stock non può superare la capacità.
- Lo storico degli acquisti contiene tutte le famiglie non fisse e non può essere inferiore alla quantità attualmente posseduta.
- Saldo ricerca non negativo; tecnologie esistenti, acquistabili e con prerequisiti chiusi, senza duplicazioni.

## Proposta di persistenza SQLite (da implementare in E3b/2)

**Non eseguire questo SQL su una partita reale.** Il seguente schema è il progetto di una futura generazione del salvataggio, non uno script di migrazione pronto:

```sql
-- Solo documentazione: futuro checkpoint v3, un'unica transazione atomica.
-- game_save / resource_stock / population_state / military_state:
-- mantenere identità, controlli e semantica dei dati già esistenti.
-- Le tabelle nuove potrebbero essere:
CREATE TABLE progression_school_state (
    slot INTEGER PRIMARY KEY CHECK (slot=1),
    school_count INTEGER NOT NULL CHECK (school_count>=0),
    waiting_students INTEGER NOT NULL CHECK (waiting_students>=0),
    qualified_researchers INTEGER NOT NULL CHECK (qualified_researchers>=0),
    active_researchers INTEGER NOT NULL CHECK (active_researchers>=0),
    research_millipoints INTEGER NOT NULL CHECK (research_millipoints>=0)
) STRICT;
CREATE TABLE progression_training (
    slot INTEGER NOT NULL CHECK (slot=1),
    position INTEGER NOT NULL CHECK (position>=0),
    progress_seconds INTEGER NOT NULL CHECK (progress_seconds>=0),
    PRIMARY KEY(slot, position)
) STRICT;
CREATE TABLE progression_family (
    slot INTEGER NOT NULL CHECK (slot=1),
    family_id TEXT NOT NULL,
    tier INTEGER NOT NULL CHECK(tier IN (0,1,2)),
    count INTEGER NOT NULL CHECK(count>=0),
    workers INTEGER NOT NULL CHECK(workers>=0),
    PRIMARY KEY(slot, family_id, tier)
) STRICT;
CREATE TABLE progression_acquisitions (
    slot INTEGER NOT NULL CHECK (slot=1),
    family_id TEXT NOT NULL,
    acquired INTEGER NOT NULL CHECK(acquired>=0),
    PRIMARY KEY(slot, family_id)
) STRICT;
CREATE TABLE progression_technologies (
    slot INTEGER NOT NULL CHECK (slot=1),
    technology_id TEXT NOT NULL,
    PRIMARY KEY(slot, technology_id)
) STRICT;
```

Requisiti implementativi per E3b/2:

1. Nuovo formato `PRAGMA user_version=3`; controllare `snapshot_version` e il ruleset insieme. Il vecchio lettore non deve interpretare il file v3 come v2.
2. **Backup S1 prima di ogni sostituzione** e lavoro solo su **copie** dei salvataggi per i test. Distinguere slot e versioni; conversione e scrittura in un'unica transazione, rollback se anche un solo vincolo fallisce.
3. Nel nuovo schema `building_stock` dovrebbe contenere soltanto `OtherBuildings`; i produttori vengono dal nuovo `progression_family`, senza doppio conteggio.
4. Validare in lettura e prima della scrittura l'intero snapshot con `V14CampaignSnapshots.Validate`, perché i `CHECK` SQLite da soli non impediscono l'assegnazione doppia di abitanti o la violazione dei prerequisiti.
5. Rifiutare dati duplicati, indici delle progressioni mancanti, tecnologie ignote, saldi negativi e timestamp fuori intervallo. Non riparare silenziosamente partite corrotte.
6. Prevedere test di caricamento v13, conversione esatta, v3 round-trip, transazione fallita, file troncato, doppio caricamento, idempotenza e salvaguardia del backup.
7. Terminare una migrazione con la *nuova* regola, non inventare contenuti a metà; non abilitare la nuova campagna prima dell'integrazione completa E1+E2+E3+E4.

## Passi successivi

- **E3b/2**: predisporre il percorso SQLite v3 (sempre separato da v13), test di round-trip, migrazione su DB temporanei e verifica di compatibilità.
- **E3b/3**: integrare `GameSession` v14, Scuola a catalogo, comandi di formazione, acquisti tecnologici e livelli; tempo unico, consumo e riserva della popolazione.
- **E3b/4**: schede Avalonia Scuola/Ricerca, prezzi per famiglia e miglioramenti; salvataggi e ripresa visualizzati correttamente.
- **E4**: benchmark economici anti-softlock, test Windows, verifica di partite reali **su copie**, solo dopo approvazione dei parametri attivazione della campagna.

## Stato della validazione

Sette nuovi test xUnit Application scritti (non eseguiti). Il toolchain .NET 10 non è disponibile in questo ambiente: eventuali problemi di compilazione/test restano da correggere alla prossima validazione. `civil-growth-13` resta l'ultima campagna realmente giocabile e v59-RC1 l'ultima release con conferma Windows.
