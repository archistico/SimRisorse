> **RC86 — dotazione nuove partite:** 2 Matite vengono accreditate soltanto dalla factory `V15FoodCampaignCheckpoints.NewCampaign`; Save/Load e `FromMigrationPlan` non alterano gli stock. SQLite generation 6 invariata. Le partite esistenti non ricevono la dotazione retroattivamente.

# Aggiornamento RC81 — v15: regime squadre nel rules_version (06/11/2026)

Nel nuovo checkpoint di runtime la proprietà `V15FoodCampaignCheckpoint.CrewMode` è **Legacy** per impostazione predefinita oppure **MintTen** dopo opt-in esplicito. SQLite generation 6 non cambia schema né `PRAGMA user_version=6`: il campo già esistente `game_save.rules_version` contiene `civil-growth-15` nel primo caso e `civil-growth-15-mint-crew10` nel secondo. Il caricamento legge e valida l'identità prima di ricostruire lavoratori/produzione; versioni sconosciute vengono rifiutate. I dati di progetto v14 e le loro directory non sono modificati. `SameCheckpoint` confronta anche la modalità prima di sostituire un file. Nessuna migrazione automatica, nessun riarmo di lavoratori o conversione inventario al cambio di regole.

[ADR 0089](adr/0089-attivazione-opt-in-squadre-zecca-v15-versionata.md) · [Piano test](validation/E4B_RC81_RUNTIME_TEST_PLAN.md). RC81 candidata, nessun test SQLite eseguito in questo ambiente.

---

> **RC71:** schema e policy di persistenza identici alla RC70 (SQLite generation 6). Lo slot fittizio `food-store` è stato eliminato soltanto dal planner in memoria. Nessun nuovo adattamento né migrazione SQLite. RC71 candidata, 822 test attesi.

# Salvataggi — contratto operativo RC70 (05/11/2026)

## Salvataggio corrente v15

- Percorso predefinito: `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v15.sqlite3`.
- **SQLite generation 6:** `PRAGMA user_version = 6`, `snapshot_version = 6`, **15 famiglie produttive** (45 righe tier Base/Grande/Avanzato); `food-store` escluso dalle famiglie persistite.
- Scrive e legge popolazione, lavoratori, civici, tecnologie, Scuola, militari, crescita, tempi frazionari, città, scorte animali, intermedi e Deposito alimentare. Cibo pronto è proprietà esclusiva del Deposito; Pesce e Pane possono stare in città e nella zona raw come scorte fisiche distinte.
- **RC69 non cambia lo schema:** SQLite generation 6 legge direttamente `V15CityCampaignSnapshot`, `V15ProductiveFamilySnapshot` e `V15PurchaseCounter`. Un eventuale `food-store` **vuoto** da importazione viene omesso prima della scrittura; se occupato, il salvataggio è rifiutato. `V14CampaignSnapshot` resta soltanto per importazione e diagnostica storica.
- È consentito **interrompere la compatibilità dei DB sperimentali**: generation 5 v15 non viene convertita automaticamente. Un DB incompatibile va rifiutato senza corromperlo; a gioco chiuso, rinominare il solo DB sperimentale se si vuole iniziare una nuova partita.

## Salvataggio personale v14: non toccare

- Percorso distinto: `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`.
- Apertura esplicita con `.\run-game.cmd --v14` (v15 è predefinita). **Nessuna conversione automatica o sovrascrittura** della partita personale v14.

## Criteri del prossimo refactoring

**RC70:** la rilettura del file SQLite temporaneo non è più un solo controllo di validità: prima del `File.Move` viene confrontato ogni campo persistito con il checkpoint canonico originario. Inventari e progressi conservano precisione al millesimo; le collezioni indicizzate sono confrontate per ID, la formazione individuale per posizione. In caso di difformità la scrittura viene rifiutata e il file precedente non viene sostituito. Le garanzie di consistenza SQLite e le copie di backup sono mantenute; questo controllo non sostituisce una prova hardware di crash-safety. `PRAGMA user_version` resta 6.

Mappatura nativa introdotta in `V15SqliteFoodSaveStore` (RC69); nessuna perdita al millesimo; round-trip e ripresa frazionaria, salvataggio atomico, rifiuto di DB vecchi e corruzioni; nessuna scrittura sul percorso v14. **Cambiare `user_version` solo se cambia il contratto sul disco**, non per un refactoring puramente in memoria.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [ADR 0069](adr/0069-generazione6-senza-famiglia-produttiva-legacy.md) · [ADR 0076](adr/0076-portafogli-contatori-v15-nativi.md).

---

## Specifica storica v14 / generazioni precedenti (riferimento, non stato della v15)

Il testo seguente descrive il writer v14 e le generazioni storiche; **non prevale sul contratto v15 sopra**.

# Uso ordinario v14 — RC31 (senza migrazione automatica)

Dal passaggio RC31 l'avvio senza argomenti apre la GUI v14, usando il **medesimo** SQLite v3 già usato in RC9–RC30: `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`. Il nome `experiments` è conservato intenzionalmente per non far sembrare perdute le partite esistenti. La v13 conserva `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3` (SQLite v2) ed è avviabile tramite `--legacy-v13`. Non si convertono file e nessun database viene spostato. Un percorso riservato dell'altra modalità viene rifiutato. [ADR 0042](adr/0042-promozione-v14-e-conservazione-slot.md).

---

> **Aggiornamento 03/11/2026 · v58-RC2 (non validata):** questo documento contiene materiale del precedente checkpoint v57 e alcune parti sono storiche. Per lo stato attuale del ramo unificato leggere [docs/CURRENT_STATE.md](CURRENT_STATE.md), [docs/ROADMAP.md](ROADMAP.md) e [docs/validation/V58_RECONCILIATION.md](validation/V58_RECONCILIATION.md). Nuovo ruleset candidato: `civil-growth-13` (addestramento individuale); flotta non implementata.

# Salvataggi desktop

**Fonte aggiornata v57 (solo documentazione):** versione del gioco `civil-growth-12`, checkpoint SQLite v2, **Municipio unico** nello stato iniziale, capacità derivata dagli edifici e `military_state` obbligatorio dalla v10 alla v12. Per il punto di ripresa completo leggere [CURRENT_STATE.md](CURRENT_STATE.md) e [NEW_CHAT_HANDOFF.md](NEW_CHAT_HANDOFF.md). Le note v53/v47 sotto descrivono il momento del loro inserimento.

**Aggiornamento v54 (28/10/2026; prevale sullo storico):** regole attive `civil-growth-12`. La capienza generale di 1.000 unità per risorsa è ora attribuita a **un Municipio fisso iniziale**, non alla base invisibile dello scenario; altri edifici aggiungono capacità specifica. Conversione S1 v11→v12 inserisce **`town-hall` conteggio 1, lavoratori 0**, senza assegnare nuove scorte; i salvataggi da v1 sono aggiornati attraverso i passi precedenti. Il caricamento rifiuta municipi mancanti, duplicati o con conteggio diverso da 1; stato militare obbligatorio anche per v12. Primo salvataggio dopo conversione: backup immutato dello stato originale. Nessuna modifica allo schema SQLite o al formato snapshot **v2**. 287 test attesi, esito Windows ancora da raccogliere. [Protocollo](validation/V54_TOWN_HALL_DISTRIBUTED_STORAGE.md).

Stato: implementato per Foundation v1 e per Civil v2, con conversione esplicita delle regole civili e copia di sicurezza. Non è compatibile automaticamente con l'esperimento M1, che usa un file e un modello differenti.

**Aggiornamento v53 (28/10/2026):** regole attive **`civil-growth-11`**; Pane ha ora **1.000 posti iniziali + 500 per ogni Magazzino**, esattamente come le altre risorse. Con la conversione S1 **v10→v11** scorte, edifici, popolazione, tempo e stato militare restano invariati. Il primo salvataggio dopo il caricamento conserva la copia originale `.civil-growth-10.backup.sqlite3`. **`military_state` è obbligatorio e validato per v10 e v11**, rifiutato nelle versioni precedenti e in versioni sconosciute. Il vecchio modello militare v10 è recuperabile tramite `CreateBarracksForBenchmarks()`, con Pane base 100. **Formato SQLite e snapshot restano v2; 280 test xUnit da eseguire su Windows.** [Gate v53](validation/V53_STORAGE_UI.md).

**Aggiornamento storico v47:** la v10 introdusse la Caserma e lo stato militare obbligatorio con migrazione v9→v10. Il test Windows di quella versione era pendente. Alcune sezioni sotto descrivono lo schema storico v8.

## Percorso e comportamento

Il nuovo default è `LocalApplicationData/SimRisorse/civil-growth.sqlite3`. Su Windows corrisponde normalmente a `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`.

Il vecchio scenario conserva `LocalApplicationData/SimRisorse/game.sqlite3` e si apre con `run-game.cmd --scenario economy-foundation`. Non viene convertito, sostituito o applicato alle nuove regole durante l'avvio del civile.

- Un singolo slot per ciascuno dei due scenari, ripreso automaticamente in pausa all'avvio dello scenario scelto.
- Salvataggio manuale con "Salva", periodico ogni minuto e alla chiusura ordinata.
- Nessun avanzamento per il tempo trascorso a programma chiuso.
- La creazione di una nuova partita richiede conferma; mantiene lo scenario corrente e sostituisce la sessione in memoria, poi il nuovo checkpoint viene scritto ai successivi salvataggi.
- Un caricamento non valido lascia la partita corrente in pausa.
- Un avvio con file non caricabile sospende l'autosalvataggio della sessione iniziale di ripiego, per non perdere il file esistente.
- Se il salvataggio in chiusura fallisce, la finestra rimane aperta; l'uscita senza salvare è una scelta esplicita del giocatore.

Il timer di autosalvataggio non fa avanzare la simulazione. Può salvare anche scelte prese in pausa; salta operazioni già in corso e conferme di nuova partita aperte.

Per sviluppo e verifiche si può scegliere un altro file:

```powershell
.\run-game.cmd --save-path "E:\percorso\partita.sqlite3"
```

La modalità `--smoke-test` usa sempre un percorso di prova, fornito dal validatore oppure generato nella cartella temporanea. Non usa il salvataggio personale predefinito.

## Identità del formato

| Identificatore | Foundation | Civil Growth |
| --- | --- | --- |
| `PRAGMA application_id` | `0x53494D52` | `0x53494D52` |
| `PRAGMA user_version` | `1` | `2` |
| Versione del contratto snapshot | `1` | `2` |
| Scenario | `economy-foundation` | `civil-growth` |
| Regole | `economy-foundation-1` | **`civil-growth-12`** (converte `civil-growth-1…-11`) |

La versione del file è distinta dalla versione del contratto e dal bilanciamento. Il loader rifiuta file estranei, versioni non supportate e regole che non corrispondono allo scenario attivo, salvo le conversioni esplicite elencate sotto. Non sono presenti migrazioni di formato o fingerprint del catalogo.

## Conversioni di regole e copie di sicurezza

Le conversioni dichiarate sono elencate in `CivilGrowthScenario.Changes` e si applicano in sequenza:

- `civil-growth-1 → civil-growth-2`: aggiunge il magazzino a zero.
- `civil-growth-2 → civil-growth-3`: aggiunge fattoria, pozzo, mulino e forno a zero.
- `civil-growth-3 → civil-growth-4`: aggiunge Casa grande a zero, introduce la risorsa distinta Pane a scorta zero e cambia la ricetta del Forno da 3 Cibo/s a 3 Pane/s. La scorta di Cibo precedente non è trasformata: non è possibile distinguerne la provenienza dal pesce o dal forno.
- `civil-growth-4 → civil-growth-5`: aggiunge il bonus di velocità per abitazione, senza nuove strutture o risorse. I secondi maturati rimangono identici; la nuova parte frazionaria in ventesimi parte a zero. Il ritmo viene ricalcolato dalle case presenti. Nessun arrivo retroattivo.
- `civil-growth-5 → civil-growth-6`: aggiunge Miniera ferro, Miniera carbone e Fonderia ferro a quantità/personale zero; Minerale ferro, Carbone e Ferro partono da zero, con capacità base 1.000 e +500 per Magazzino già posseduto. Conserva tempo, scorte civili, persone, edifici e frazione di crescita. Vecchi checkpoint che dichiarano già risorse industriali positive o righe industriali incompatibili sono rifiutati. Politica **S1 scelta dall'utente**, [ADR 0014](adr/0014-first-iron-industry-and-civil-save-upgrade.md).
- `civil-growth-6 → civil-growth-7`: aggiunge Miniera e Fonderia d'oro a quantità/personale zero; Minerale d'oro e Oro a zero, capacità 1.000 +500 per Magazzino già posseduto. **Ferro, Carbone, lavoratori e progresso già esistenti si conservano**. Rifiuta scorte d'oro positive antecedenti alla loro introduzione. SQLite v2 e politica S1 invariati; [verifica su checkpoint v6](validation/M5_1B_GOLD.md).
- `civil-growth-7 → civil-growth-8`: aggiunge **Forgia** a quantità/personale zero e la risorsa **Spade** a zero. Capacità Spade 1.000 +500 per Magazzino già posseduto. Il converter rifiuta Spade positive dichiarate da un salvataggio v7. Tutto lo stato precedente è mantenuto, senza produzione retroattiva; stesso formato SQLite v2, backup originale al primo salvataggio. [Verifica v8 da eseguire](validation/M5_2A_FORGE.md).
- `civil-growth-8 → civil-growth-9`: aggiunge **Zecca** a quantità/personale zero e la risorsa **Monete** a zero; rifiuta Monete positive dichiarate da un salvataggio v8, conserva i dati precedenti.
- `civil-growth-9 → civil-growth-10`: aggiunge **Caserma** a quantità zero e lo **stato militare vuoto**. Le regole v9 non avevano stato militare: eventuali dati militari preesistenti in un checkpoint che si dichiara v9 vengono rifiutati. La popolazione, gli addetti, le scorte, il tempo frazionario e il progresso della crescita restano invariati. Non vengono inventate reclute o Spade. Per una partita **già v10**, `military_state` deve esistere ed essere valida; nessun default silenzioso.
- `civil-growth-10 → civil-growth-11`: aumenta la **capacità iniziale del Pane da 100 a 1.000**, senza alterare i quantitativi già salvati né aggiungere edifici. Ogni Magazzino mantiene il bonus +500 identico per tutte le risorse. Conserva **anche** coda, addestramento, soldati, orologio e frazioni; non reinterpreta le reclute. La nuova capienza è calcolata dal catalogo e non inserita nel checkpoint.
- `civil-growth-11 → civil-growth-12`: aggiunge **un Municipio (`town-hall`) con conteggio 1 e 0 addetti**. La precedente base invisibile di 1.000 per risorsa è sostituita dal contributo del Municipio; i produttori aggiungono 50 posti ai soli output, le Case 10/25 a Cibo e Pane, le Caserme 20 a Spade, i Magazzini 500 a tutte. **Non cambiano le quantità delle scorte**, le persone, il tempo, l'esercito né i progressi frazionari. Vietati Municipio assente, plurimo o con addetti; la prima sovrascrittura conserva il backup S1. Gli inventari rimangono condivisi.

Il convertitore rifiuta un checkpoint che dichiara `civil-growth-3` ma possiede già Casa grande o una scorta positiva di Pane. Il caricamento non applica una nuova simulazione per il passato. Formato file e snapshot restano **v2**: ogni edificio è una riga in `building_stock`, mentre le nuove risorse a zero non hanno righe. Nelle **regole v11** tutte le capacità di base sono **1.000**; nel catalogo storico fino alla **v10** Pane era l'eccezione con 100. Ogni Magazzino aggiunge **500** a ciascuna capacità; le capacità sono calcolate dagli edifici e non salvate. Decisioni negli [ADR 0010](adr/0010-storage-capacity-and-rules-conversion.md) e [0011](adr/0011-advanced-housing-and-bread-rations.md).

Il caricamento converte solo in memoria. Il primo salvataggio che sostituirebbe un checkpoint della stessa generazione con scenario o regole diversi copia prima il file con `VACUUM INTO` in `<nome>.<regole>.backup.sqlite3`, nella stessa cartella (per esempio `civil-growth.civil-growth-1.backup.sqlite3`). Se il nome esiste si usa `-2`, `-3`…: le copie non vengono sovrascritte e, senza un nome libero, il salvataggio fallisce. I caratteri delle regole non sicuri per un percorso diventano `_`. Le copie si riaprono con `run-game.cmd --save-path`, con un programma che supporti quelle regole.

Nelle due generazioni implementate, versione del file e dello snapshot devono corrispondere. Il writer rifiuta anche la sovrascrittura tra v1 e v2: per generazioni diverse si usano percorsi separati. Decisione nell'[ADR 0009](adr/0009-civil-growth-and-save-generations.md).

## Schema SQLite v1

Tre tabelle `STRICT`, con quantità intere e controlli di non negatività:

| Tabella | Colonne principali | Responsabilità |
| --- | --- | --- |
| `game_save` | slot, snapshot_version, scenario_id, rules_version, elapsed_ticks, population | Identità e avanzamento |
| `resource_stock` | slot, resource_id, milliunits | Scorte esatte in millesimi |
| `building_stock` | slot, building_id, count, workers | Edifici e assegnazioni |

`slot` identifica il checkpoint `1`. Chiavi composte impediscono duplicati di risorse o edifici; i riferimenti allo slot sono vincoli esterni. Le scorte nulle possono essere omesse e vengono ricostruite come zero. Lo snapshot contiene invece tutti i tipi di edificio, inclusi quelli con quantità zero.

`elapsed_ticks` conserva unità di 100 ns, compreso il residuo inferiore al secondo. Non viene salvato un timestamp da utilizzare per produzione offline.

## Estensione SQLite v2

**Estensione v47/v53/v54:** per le regole **`civil-growth-10`, `civil-growth-11` e `civil-growth-12`** è obbligatoria una tabella `military_state` con una riga `slot=1` e colonne `waiting`, `training`, `soldiers`, `progress_seconds` (interi `STRICT` non negativi). Il salvataggio avviene **nella stessa transazione** di `game_save`, `resource_stock`, `building_stock` e delle tabelle di crescita; un errore annulla tutte le modifiche. Per le versioni precedenti alla v10 la tabella può essere assente, ma non può contenere una riga militare riferita al checkpoint. La convalida applicativa controlla inoltre l'effettiva capienza delle Caserme, la durata del gruppo e l'invariante **popolazione = civili liberi + addetti + reclute in coda + reclute in formazione + soldati**.

Le tre tabelle principali restano presenti. Il file v2 ha una tabella obbligatoria per la crescita, una tabella di frazione compatibile con i precedenti checkpoint e, dalle regole v10 in poi, anche la tabella militare obbligatoria descritta sopra:

| Tabella | Colonne | Responsabilità |
| --- | --- | --- |
| `population_state` | slot, growth_seconds | Secondi validi verso il prossimo abitante |
| `population_growth_fraction` | slot, twentieths (0–19) | Frazione di progresso verso il prossimo abitante, in ventesimi di secondo |

`slot=1` è chiave primaria e riferimento a `game_save`. `growth_seconds` è un intero non negativo; il Core verifica che sia inferiore all'intervallo delle regole attive (60 per Civil). Una riga mancante non significa progresso zero: il checkpoint è incompleto e viene rifiutato. Lo snapshot v2 richiede `GrowthProgressSeconds`; lo snapshot v1 non deve contenerlo.

Il vecchio checkpoint SQLite v2 **senza la tabella `population_growth_fraction`** è ancora leggibile: il resto viene interpretato come zero. Se la tabella è presente, la riga per lo slot 1 è obbligatoria e controllata. Il nuovo writer la crea e salva assieme alle altre tabelle nella stessa transazione, senza cambiare `user_version=2`. Snapshot e Core espongono `GrowthRemainderTwentieths` (0–19); le regole storiche `civil-growth-4` richiedono resto zero. Nessun calcolo usa numeri a virgola mobile.

`game_save.population` conserva il numero effettivo delle persone, non soltanto la dotazione iniziale. Il Core verifica anche capacità abitativa, popolazione iniziale minima dello scenario senza decessi e assegnazioni. Il nuovo abitante, i relativi consumi e il progresso residuo non vengono ricalcolati dal tempo totale.

Il formato limita a 10.000 le righe di risorse e quelle di edifici per checkpoint. Questo è un limite tecnico del primo adattatore, non un numero di strutture costruibili: il campo `count` aggrega molte costruzioni dello stesso tipo.

## Atomicità e controlli

Metadati, crescita v2, rimozione delle vecchie righe e inserimento del nuovo stato fanno parte di una sola transazione. Un errore nell'ultima fase annulla anche le modifiche precedenti. Il caricamento legge i gruppi di dati nella stessa transazione differita, così non mescola checkpoint diversi.

L'adattatore controlla identità del file, forma dei dati, limiti e quantità. Il caso d'uso applicativo passa poi lo snapshot al Core per verificare catalogo, popolazione, capacità e assegnazioni prima della sostituzione. Un oggetto costruito da chiamanti esterni del port deve comunque rispettare gli invarianti di dominio: il writer non conosce tutte le regole dello scenario.

I database estranei e le versioni di file o snapshot future non sono sovrascritti. Per lavorare con uno stato incompatibile mantenendo l'originale si usa un percorso differente; non si aggiorna silenziosamente una vecchia partita alle regole correnti.

## Concorrenza e gestione degli errori

`SaveGameHandler` cattura uno snapshot coerente sotto il coordinatore, poi svolge I/O fuori dal lock. Un gate distinto serializza salvataggi e caricamenti. Il salvataggio cattura lo stato dopo l'attesa del gate.

`LoadGameHandler` mette in pausa, registra una revisione e carica fuori dal lock. Al completamento sostituisce la sessione solo se nessun altro comando o avvio ne ha invalidato la revisione.

Il port espone errori applicativi: file assente, formato non supportato, regole incompatibili, dati invalidi, indisponibilità, operazione superata o annullata. Il driver SQLite non è esposto nei ViewModel. L'annullamento non interrompe necessariamente una scrittura sincrona già iniziata.

## Verifiche e limiti

Round trip, precisione, ricaricamento in pausa, rifiuto di versioni future e database estranei, rollback mediante trigger ed errori di chiusura sono verificati nel [report M3](validation/M3_DESKTOP.md). Ripresa esatta della crescita, rollback v2, stato parziale corrotto e protezione tra generazioni sono nel [report M4](validation/M4_CIVIL_GROWTH.md). La fixture M3 rimane leggibile in v1.

Non sono ancora implementati più slot per scenario, selezione grafica degli scenari, gestione delle copie dalla UI, migrazioni di formato, conversioni non dichiarate in `CivilGrowthScenario.Changes` o validazione competitiva per una classifica pubblica. Un arresto forzato può perdere il progresso successivo all'ultimo checkpoint; non è stato simulato uno spegnimento elettrico.

**Stato v57 (codice v55):** Ferro, Carbone, Oro, **Spade e Monete** sono risorse integrate; **Forgia, Zecca e Caserma/Esercito** sono già implementate. Le regole S1 non hanno cambiato il formato SQLite v2. **Navi, conquiste e Castello** non sono implementati e richiedono ulteriori contratti espliciti.


## Candidato SQLite v15 alimentare (RC46 / F1-D05b)

**Questo formato è separato e non è utilizzato dal launcher v14.** Lo store `V15SqliteFoodSaveStore` usa `user_version=4`, regole esterne `civil-growth-15`, e le tabelle `food_raw_stock` e `food_ready_stock` oltre agli stati esistenti. La tabella `resource_stock` contiene soltanto risorse non alimentari. L’involucro `CityCampaign` conserva i campi v14 per compatibilità interna, non per tornare a scrivere sul file v14.

Tutte le quantità sono millesimi interi e non negativi. Il deposito conserva anche scorte storiche eccedenti la futura capienza; senza lavoratori o nuovo motore attivo nessuna lavorazione avviene automaticamente. Il writer valida in memoria, scrive un file temporaneo, riapre per verificarlo e prima di sovrascrivere un altro **v15 valido** ne salva un backup SQLite consistente. File di generazione diversa non vengono sovrascritti.

F1-D05c / RC47 aggiunge un migratore **copy-on-write esplicito** con controllo della conservazione e idempotenza su destinazione identica. Il salvataggio v14 resta l'originale per rollback; lo switch nella campagna e il relativo backup aggiuntivo sono rimandati a F1-D05d/e. Nessun caricamento automatico. [Store](design/F1_D05B_RC46_PERSISTENZA_SQLITE_V15.md), [contratto di migrazione](design/F1_D05C_RC47_MIGRAZIONE_COPY_ON_WRITE.md) e [ADR 0058](adr/0058-migrazione-v14-v15-senza-sovrascrittura.md).


## RC90 — Depositi alimentari per tier, formato SQLite v15 invariato

Il salvataggio v15 mantiene `PRAGMA user_version=6` e la tabella `food_industry_site` senza nuove colonne. Al fianco delle sei righe canoniche, le nuove partite con RC90 scrivono `food-depot-large` e `food-depot-advanced` (conteggio edifici e lavoratori assegnati). La lettura accetta **esattamente sei** righe dei checkpoint precedenti oppure **otto** righe complete; con sei righe, entrambi i livelli aggiuntivi valgono zero. Un gruppo parziale di sette righe è invalido. La ricerca acquistata viene conservata nella tabella esistente `progression_technologies`: un deposito di livello superiore non può risultare costruito senza il relativo sblocco. V14 non cambia.


## Nota RC93 — database sperimentale separato

Il file SQLite `E4bCurveAFundingSqliteStore` non è un salvataggio di partita: usa `application_id=0x53465239` e `user_version=1` **solo nel suo database isolato**. La campagna v15 continua a usare il proprio schema **generation 6** senza modifiche. Non puntare lo store di finanziamento verso un salvataggio reale; il costruttore rifiuta formati estranei. I rifornimenti del sandbox sono ipotetici. [ADR 0103](adr/0103-persistenza-sandbox-finanziamento-atomica-idempotente.md).
