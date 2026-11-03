# SimRisorse — Audit di riallineamento fra ZIP modificato e v57

**Data:** 3 novembre 2026  
**Oggetto:** confronto statico di `/mnt/data/SimRisorse.zip` (ramo v41, modificato fino al 03/11) e `SimRisorse-v57.zip` (checkpoint documentale v57, codice v55).  
**Decisione tecnica:** **non** sovrascrivere la v57 con il contenuto della v41. Congelare gli originali e integrare le innovazioni militari in una nuova versione delle regole a partire dalla v57.  
**Limite dell'audit:** non è disponibile l'SDK .NET in questo ambiente: nessuna compilazione, nessun test xUnit o smoke Windows è stato eseguito durante l'audit.

## 1. Integrità e confronto effettivo

| Proprietà | ZIP caricato (ramo v41) | ZIP di riferimento (v57) |
|---|---:|---:|
| Dimensione ZIP | 1.617.454 byte | 1.701.751 byte |
| File effettivi (senza directory) | 237 | 255 |
| Ruleset attivo | `civil-growth-10` | `civil-growth-12` |
| Strutture in catalogo | 20 | 21 (20 per la campagna + Municipio fisso) |
| Risorse | 16 | 16 |
| Casi xUnit dichiarati/attesi nel pacchetto | 308, **non eseguiti** | 287, **non attestati per il checkpoint** |
| Ultimo gate precedente riportato dal rispettivo ramo | 260/260 sulla v40, prima dell'intervento M5.3 | storico e conferme utente, gate corrente v55/v57 pendente |

Al netto della directory principale `SimRisorse/` presente soltanto nel primo archivio, risultano **53 file comuni modificati**, **12 esclusivi del ramo v41** e **30 esclusivi della v57**. L'inventario completo è incluso in `DIFF_MANIFEST_SIMRISORSE.json`.

### Hash SHA-256 degli archivi originali

- ZIP caricato: `2f7ee7d2a5ca0a273bca935518737734e9a4532c273dffb45755e76bfacf53a1`
- ZIP v57: `2a8c07cd73211887a18f094d7bf3182a4f932fdc9a88452c368a1655649850bb`

## 2. Che cosa è nuovo e importante nel ramo v41

La v41 documenta un nuovo contratto di Caserma, deciso il 03/11 e sviluppato in `src/SimRisorse.Core/Military/MilitaryRules.cs`, `MilitaryState.cs` e nella sessione di gioco:

| Elemento | v57 | Modifica v41 |
|---|---|---|
| Costo Caserma | 280 Legno lavorato, 200 Pietra | **260 Legno lavorato, 200 Pietra** |
| Prerequisito | Forgia | Invariato |
| Spade per recluta | 2 | **5** |
| Addestramento | Gruppi di max 2 posti/Caserma, 60 secondi/gruppo | **1 recluta per posto/Caserma, 30 secondi individuali** |
| Annullamento | Solo reclute in attesa, rimborso Spade se possibile | **Anche reclute in addestramento**, priorità alla coda e poi a chi ha meno progresso; rimborso Spade completo se capacità sufficiente |
| Congedo | Soldati pronti congedabili senza rimborso | **Assente; deliberatamente rinviato alle missioni** |
| Popolazione | Reclute e soldati restano abitanti, occupano case e consumano razioni | Invariato |

**Conseguenza economica:** a regime entrambe le varianti consentono teoricamente 2 soldati/minuto per Caserma; la v41 consegna i soldati uno per volta e richiede 2,5 volte le Spade per persona. Il costo della Caserma diminuisce di 20 Legno lavorato. La capacità di annullare l'addestramento è una meccanica nuova significativa.

Altre nuove attività presenti nel ramo v41:

- Stato militare con progressi individuali ordinati e validazione dei checkpoint; arruolamento/annullamento atomici.
- Proiezione militare con posti, previsioni di completamento, numero massimo arruolabile e massimo annullabile.
- Nuove tabelle `military_state` / `military_training` in SQLite, persistenza di coda e progressi individuali.
- UI della Caserma con comandi Arruola e Annulla, icone e dettagli aggiornati.
- Cronologia `military-8-hours`, strategia `military` e **48 casi aggiuntivi previsti**, documentati in `docs/validation/M5_3A_BARRACKS.md`.
- ADR `docs/adr/0017-barracks-recruitment-and-v9-save-upgrade.md` che cristallizza il nuovo modello.

**Stato effettivo:** implementato nei sorgenti; non verificato tramite build/test della v41.

## 3. Funzionalità già presenti nella v57 ma mancanti nel ramo v41

### 3.1 Economia e capacità — da ripristinare senza eccezioni

1. **Municipio fisso iniziale** (`town-hall`): edificio unico e non acquistabile, +1.000 capacità per **ciascuna** delle 16 risorse. La base invisibile dello scenario passa a zero; le scorte e strutture iniziali restano quelle originali.
2. **Stoccaggio distribuito:** +50 capacità per ogni risorsa prodotta da ciascun edificio produttivo; le strutture non produttive ricevono bonus specifici (case +10/+25 Cibo e Pane; Caserma +20 Spade). Ogni Magazzino resta +500 per ciascuna risorsa.
3. **Pane uniformato:** la capacità iniziale del Pane non è più la vecchia eccezione da 100; Municipio e Magazzino applicano le medesime quantità delle altre risorse.
4. **Protezione dell'edificio fisso:** `IsScenarioFixed` nei modelli, verifica all'inizializzazione e al ripristino e rifiuto della costruzione. Il ramo v41 non prevede questo vincolo: sostituire direttamente `GameSession` o `Definitions` rischierebbe di rendere costruibile un Municipio gratuito, replicandone la capacità.

### 3.2 Interfaccia — da preservare sulla v57

5. **Spiegazione della capacità** al sorvolo della colonna Capacità nella pagina Risorse.
6. **Capacità degli edifici** in dettaglio con stile/icone coerenti con Costi, Produzioni e Consumi.
7. **Scheda Esercito** separata, con navigazione specifica e visibilità corretta nella Foundation; nel ramo v41 la gestione è concentrata sulla selezione Caserma.
8. **Diagnosi e suggerimenti** per blocco da capacità insufficiente e messaggi con indicazione di come aumentarla.

Restano già presenti nella v41 diversi miglioramenti storici, come ordinamento alfabetico, selezione, stima `Si riempie tra`, quantità arrotondate, testo compatto, palette retro: non occorre attribuire questi interventi esclusivamente alla v57.

### 3.3 Salvataggi e protezione dei dati

9. **Versioni `civil-growth-11` e `-12`**: uniformità del Pane e introduzione del Municipio, con migrazioni ordinate, backup S1 e preservazione dei progressi. Questi step mancano nella v41.
10. **Factory storiche v10/v11/v12** congelate: indispensabili per conversione, confronti regressivi e compatibilità dei checkpoint.
11. **Test aggiuntivi** per Municipio, capacità, GUI e protezione del catalogo. Sono un insieme non sovrapponibile ai 48 nuovi casi della v41: i totali 308 e 287 non si possono sommare senza esaminare duplicati e sostituzioni.
12. **Documentazione autorevole v57:** `00_START_HERE.md`, `docs/CURRENT_STATE.md`, `docs/AUTHORITY_REGISTRY.md`, `docs/CATALOG_CURRENT.md`, registri di handoff, scheda navale `M5_4_DECISION_GATE_V55.md` e verifiche v53-v55; non vanno persi.

## 4. Rischi bloccanti dell'unione meccanica dei due ZIP

| Priorità | Rischio | Evidenza | Soluzione proposta |
|---|---|---|---|
| **P0** | **Ruleset `civil-growth-10` riutilizzato con due significati militari differenti** | v57: 2 posti × 60 s, 2 Spade; ramo v41: 1 posto × 30 s, 5 Spade | **Nuovo ruleset `civil-growth-13` per il cambiamento militare**. Le factory v10-12 restano intatte; Cantiere slitta alla successiva versione. Non reinterpretare vecchi snapshot in modo silenzioso. |
| **P0** | **Schema SQLite militare incompatibile con lo stesso nome di tabella** | v57 `military_state(waiting, training, soldiers, progress_seconds)`; v41 `military_state(queued_recruits, soldiers)` + `military_training` | Rilevare gli schemi e implementare una lettura/conversione dichiarata; non affidarsi a `CREATE TABLE IF NOT EXISTS` con colonne diverse. Eseguire test su copie di file reali dei due rami. |
| **P0** | **Perdita del Municipio o possibilità di duplicarlo** | nuova `CivilGrowthScenario` non ha `town-hall`; la nuova `GameSession` non applica `IsScenarioFixed` | Conservare la definizione e tutte le protezioni della v57. |
| **P0** | **Regressione capacità e Pane** | ramo v41 `bread=100` iniziale e capacità di base; v57 Municipio+produttori+case | Mantenere interamente il modello stoccaggio v57 e adattare solo le nuove regole militari. |
| **P1** | **Scomparsa di scheda Esercito/tooltip/effetti UI** | layout e ViewModel del ramo v41 non includono le aggiunte v55 | Integrare i comandi nuovi nella UI v57, non copiare i file AXAML in blocco. |
| **P1** | **Congedo rimosso intenzionalmente** | v57 `DischargeSoldiers` vs v41 rinvio | Confermare che la decisione del 03/11 prevalga; in tal caso disabilitare l'azione nel nuovo ruleset, conservando le verifiche storiche. |
| **P1** | **Report e verificatore discordanti** | il validatore v41 contiene attese precedenti per Pane, capacità e versioni | Aggiornare attese ed esperimenti rispetto alla v57 senza indebolire le regressioni storiche. |
| **P1** | **Nessuna compilazione accertata della v41** | `docs/HANDOFF.md` e rapporto M5.3 | Eseguire `validate.cmd` su Windows prima/dopo integrazione e correggere eventuali errori. |

### Nota importante sui salvataggi esistenti

La modifica a `civil-growth-13` dovrà convertire le partite v12 mantenendo scorte, abitanti, edifici, navi (non esistono ancora), progressi e militari. **Il tempo di addestramento non è direttamente equivalente** tra il vecchio gruppo e i nuovi posti individuali. Occorre una policy esplicita: ad esempio preservare soldati già formati e reclute, ma riavviare il progresso delle reclute in addestramento; oppure definire una mappatura proporzionale controllata. Non scegliere di nascosto. Chi avesse avviato la v41 potrebbe anche possedere file `civil-growth-10` con lo schema militare alternativo: riconoscerli e gestirli separatamente, con backup prima di qualsiasi scrittura.

## 5. Cose già pianificate nella v57, ma ancora assenti in **entrambi** i rami

| Milestone | Cosa è già pianificato | Stato |
|---|---|---|
| **G1** | Regole di esplorazione separata da conquista, requisiti deterministici, nessuna perdita irreversibile inizialmente; progettazione quantitativa da chiudere | **Base di discussione** in chat; non implementata negli ZIP |
| **M5.3c** | Misurare l'impatto civile di reclutamento precoce/tardivo, piccoli/grandi contingenti, Cibo, Pane e produttività | Studio/benchmark da completare |
| **M5.4a** | Cantiere navale, acquisto navi persistenti con ID univoci, costi e requisiti da approvare | Non implementato |
| **M5.4b** | Salvataggio della flotta, interfaccia Flotta e compatibilità S1 | Non implementato |
| **M5.4c** | Preflight capacità navi/soldati, senza consumare o assegnare risorse | Non implementato |
| **M5.5** | Esplorazione, viaggi temporizzati col tempo simulato, assegnazione esclusiva delle navi, ripresa e ritorno | Non implementato |
| **M5.6** | Conquista di almeno una isola, requisiti verificabili, risultato persistente e senza doppia applicazione | Non implementato |
| **M5.7** | Castello dopo una conquista, ultimo dei 22 edifici-obiettivo | Non implementato |
| **M6** | Vittoria, classifica/statistiche, percorso completo misurato e bilanciamento obiettivo ~8 h / 32+ h | Non implementato |
| **M7** | Rifinitura, DPI, accessibilità, smoke e rilascio riproducibile | Non completato |

La proposta navale N-01…N-07 della v57 **non è stata approvata**. Costi ipotizzati: Cantiere 320 Legno lavorato + 220 Pietra, nave 160 Legno lavorato + 120 Monete, capacità 5 soldati; sono ipotesi da confermare dopo il consolidamento militare. L'indirizzo esplorazione separata/conquista deterministica/nessuna perdita irreversibile resta una **base di discussione**, non un'autorizzazione automatica a sviluppare.

## 6. Piano raccomandato, in ordine vincolante

### R0 — Protezione degli originali e fotografia delle versioni

- Tenere immutati `SimRisorse.zip` e `SimRisorse-v57.zip`, registrare hash SHA-256 e costruire una nuova directory di lavoro.
- Nessuna operazione sui salvataggi personali in `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`.
- Se possibile, eseguire i due validatori Windows in cartelle temporanee separate per misurare lo stato reale dei rami (con test/saves isolati).

### R1 — Congelare il contratto militare scelto

- Confermare che la decisione v41 del 03/11 sia quella definitiva: **260/200**, **5 Spade**, **1 posto/Caserma**, **30 s/recluta**, cancellazione anche durante addestramento, **congedo rinviato**.
- Risolvere la conversione del progresso di addestramento delle vecchie partite e il supporto ai file militari del ramo v41 eventualmente già creati.

### R2 — Integrare il nuovo modello nel ramo v57 **senza sostituire file interi indiscriminatamente**

- **Core:** aggiungere le regole individuali e lo stato militare nuovo, preservando `IsScenarioFixed`, capacità distribuite e validazione rigorosa. Se serve, mantenere il motore militare storico per riprodurre i vecchi checkpoint.
- **Application:** nuovo ruleset `civil-growth-13` specificamente militare; v10/v11/v12 congelate e migrazioni dichiarate; proiezioni militari compatibili con la scheda Esercito v57.
- **Infrastructure:** doppia lettura del formato militare legacy e del nuovo formato; conversione esplicita, scrittura atomica, backup S1 verificato; evitare conflitti di colonne di `military_state`.
- **Desktop:** mantenere tab Esercito, Municipio, tooltip capacità, dettagli edifici e lista Risorse; integrare le nuove azioni di arruolamento/annullamento con icone e tempi.
- **Tests/Simulator:** recuperare il valore dei nuovi casi v41, adattarli alla base v57 e conservare tutti i test di non regressione v57. Il numero finale si potrà conoscere solo dopo la deduplicazione e l'esecuzione.

### R3 — Gate di accettazione del ramo consolidato

Test obbligatori: avvio Municipio unico e non costruibile; capacità iniziali e del Pane; fabbisogno alimentare costante con arruolamento; 1 recluta per 30 s/Caserma; parallelismo fra più Caserme; annullamento durante addestramento e rimborso atomico; conteggi civili; confronti passi 1 s e frazionari; salvataggi v9, v10, v11, v12 e v41-schema alternativo; rollback e backup; tutte le schermate; smoke Windows. **Non affermare successo prima di un gate `validate.cmd` realmente superato**.

### R4 — Ripresa della roadmap

Dopo il gate: M5.3c impatto civile → finalizzare G1 e contratto N-01..N-07 → M5.4 Cantiere/navale sul **ruleset successivo** (candidato `civil-growth-14`) → M5.5 esplorazione → M5.6 conquista → M5.7 Castello → M6 bilanciamento/vittoria → M7 rilascio.

## 7. Decisioni che richiedono conferma prima di cambiare il codice

1. **La variante militare del 03/11 sostituisce definitivamente quella della v57?** (Questo è il presupposto più plausibile, da confermare formalmente.)
2. **Congedo:** lo rinviamo davvero, come nel nuovo ZIP, oppure manteniamo il congedo già operativo nella v57?
3. **Migrazione progresso militare:** conservare integralmente il numero di reclute/soldati ma azzerare il progresso delle reclute in addestramento è semplice e prevedibile; una trasformazione proporzionale richiede una regola ulteriore e test. Scelta non ancora approvata.
4. **Hai già giocato e salvato con la v41 modificata?** Se sì, è obbligatorio testare anche la variante di schema SQLite che si trova nel nuovo ZIP.

## 8. Stato finale dell'audit

**Accertato staticamente:** differenze e file, codici militari, struttura SQLite, catalogo, regole del Municipio/capacità e funzionalità programmate.  
**Non accertato eseguendo il software:** compilazione e comportamento runtime dello ZIP v41, qualità dei render, buon esito dei 308 test, conversioni reali dei salvataggi e prestazioni di bilanciamento.  
**Consegna:** diagnosi e proposta di integrazione; **nessuna modifica ai sorgenti o agli originali**.
