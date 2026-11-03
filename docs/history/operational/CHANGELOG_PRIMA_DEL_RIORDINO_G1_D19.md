## 2026-11-03 — v60-E3b4-RC3, G1-D18: ripristino delle funzioni della GUI classica

- RC2 non approvata visivamente: Risorse ridotta a tre colonne, Esercito solo riepilogo, Costruisci su edifici bloccati. La RC3 corregge queste regressioni.
- Scheda Risorse: stesso XAML a sette colonne e stesso `ResourceRow` della v13; `V14EconomyProjector` read-only produce produzione, consumo e saldo tramite i pianificatori effettivi.
- Crescita e popolazione tornano nella sidebar; capacità condivisa e tooltip per edificio/livello.
- Esercito: stessa scheda v13, arruola/annulla con regole militari originali, congedo vietato dove previsto.
- Edifici bloccati senza Costruisci; materiali insufficienti disabilitano, con diagnosi; quote per risorsa con icona. Ordini +1/+5/+10 senza ComboBox di livello o lotto.
- Nuova partita con conferma e autosalvataggio v14 ogni minuto, sempre su DB separato.
- Test nuovi: sei Desktop e due Application. **439 attesi**; compilazione/test .NET non eseguiti qui. Verifica visiva ancora obbligatoria.
- `civil-growth-13`, `GameSession`, SQLite v2 e GUI v13 originali **invariati**.

# v60-E3b4-RC2 · E3b/4b · 03/11/2026

- **RC1 validata: 427 test superati** dall'utente; revisione GUI richiesta.
- Layout sperimentale v14 sostituito con schema della GUI originale: sidebar scura, elenco Edifici, dettaglio 310px, riepilogo popolazione/risorse; Scuola e Albero tecnologico nel menu di sinistra.
- 16 famiglie × 3 voci edificio produttivo, più civili/Scuola; tier selezionato dalla riga, **senza ComboBox** nel dettaglio. Azioni dirette per costruire e potenziare.
- Capacità indicate come +50/+150/+500 per ciascuna risorsa prodotta; ricerca e prerequisiti mostrati quando mancanti.
- Nuovi test Desktop (+3; 430 attesi), aggiornamento test GUI precedente. **Non compilati né eseguiti** qui.
- Core, Application, Infrastructure, `MainWindow`, `MainViewModel`, SQLite v2/v3 e campagna v13 **invariati**.
- ADR 0032, D17, roadmap, piano test e checkpoint. Directory principale pulita.

---

# v60-E3b4-RC1 · E3b/4a · 03/11/2026

- RC5 validata sul PC dell'utente (417 test passati).
- Nuova finestra v14 sperimentale (Scuola, formazione, ricerca, tier, potenziamenti, civili, inventario) con avvio esplicito `--experimental-v14`.
- Salvataggio SQLite v3 in percorso separato, caricamento di snapshot v14 validato e blocco dei file incompatibili; nessuna migrazione automatica dalla v13.
- Nuovo viewmodel e 10 test Desktop (427 attesi, da eseguire con .NET 10 su Windows).
- Core/Application/Infrastructure/MainWindow e script **invariati**. Radice ordinata, documenti storici in `docs/history/`.
- Documentazione: ADR 0031, D16, checkpoint e piano test, README/stato/roadmap semplificati.

---

## v60-E3b3-RC5 — 03/11/2026 (candidato E3b/3e)

- Ultimo gate confermato: RC4, 408 test verdi secondo l'utente.
- Comando `BuildCivicBuilding` per Case, Magazzini, Caserme, con aumento prezzi per acquisto e commit atomico.
- Snapshot/SQLite v3 invariati; test di replay misto costruzione / ricerca / tier / clock / save-restore.
- 7 test Application + 2 Infrastructure, obiettivo 417, da validare Windows.
- Nuovi rapporto D15, ADR 0030, piano test, checkpoint e manifest; directory principale pulita.
- `civil-growth-13`, SQLite v2 e GUI non modificati; prezzi candidati.

---

## v60-E3b3-RC4 — 03/11/2026 (candidato, da testare)

- Ultimo gate confermato: RC3 con 396/396 test Windows superati secondo l'utente.
- Aggiunto `V14TierConstructionRules` e risultati tipizzati per comandi di costruzione e staffing nel Core isolato.
- Collegati i calcolatori E1/E2 alla `V14CampaignSession`: costruzione Base/Grande/Avanzato, upgrade diretto o per gradi, assegnazione e rilascio lavoratori.
- 11 test Application + 1 test Infrastructure; **408 test attesi**, .NET non disponibile nel container.
- `civil-growth-13` / UI / SQLite v2 / motore militare invariati, directory principale ordinata, snapshot v14/SQLite v3 senza cambiamenti di formato.
- Nuova specifica G1-D14, ADR 0029, checkpoint e piano test. Prossimo E3b/3e replay esteso.

---

## v60-E3b3-RC3 — Fix fixture di popolazione nei test del clock v14 (3 novembre 2026)

- Windows RC2: 396 test eseguiti, 389 superati, 7 falliti per configurazione non valida in `V14CampaignClockTests.Prepared()`.
- Corrette le fixture: 10 residenti per 3 lavoratori iniziali + 2 addetti alimentari + 1 militare + 4 studenti/ricercatori; 2 Case piccole garantiscono 16 posti.
- Scenario con 10 lavoratori nell'impianto avanzato richiede 13 residenti contando i 3 lavoratori iniziali.
- Aggiunta verifica `AvailablePopulation == 0` alla fixture. Nessun nuovo test; obiettivo invariato 396. Nessuna modifica a `src/` né a `GameSession`, SQLite o UI.
- RC3 non compilata/testata nel container; validazione Windows ancora da confermare. [Rapporto](../../validation/V60_E3B3_RC3_FIX_CLOCK_FIXTURE.md).

---

## v60-E3b3-RC2 — E3b/3c clock v14 isolato (3 novembre 2026)

- Confermato dall'utente il superamento di `validate.cmd` sulla E3b3-RC1 (387 test attesi).
- Nuovo adattatore `V14CampaignClock` per produzione, razioni, crescita, militari, formazione studenti e conoscenza sullo stesso secondo simulato.
- `V14CampaignSession` ora gestisce pausa, resume e Advance atomico, inclusi tick parziali e limiti come `GameSession`.
- 9 nuovi casi xUnit (8 Application + 1 Infrastructure SQLite), target 396 test; **non eseguiti in questo ambiente**.
- Nessuna modifica a `GameSession` v13, UI, SQLite v2, catalogo storico, benchmark o script. ADR 0028 e G1-D12.

---

## v60-E3b3-RC1 — E3b/3a-b sessione v14 isolata (3 novembre 2026)

- RC3 SQLite v3 validata dall'utente con `validate.cmd` interamente passato; questo è un nuovo candidato **non testato con .NET**.
- Aggiunta `V14CampaignSession` con bootstrap/restauro, Scuola costruibile nel prototipo, transizioni atomiche studenti/ricercatori, acquisti espliciti di tecnologie.
- Aggiunti `V14SchoolConstructionRules` con parametri di esempio, non approvati come bilanciamento definitivo.
- Aggiunte 2 suite e 7 test per invarianti, costi, popolazione e round-trip SQLite v3. Partita v13, SQLite v2 e UI invariati.
- ADR 0027 sperimentale, G1-D11, aggiornamento roadmap e handoff. [Dettaglio](../../design/G1_D11_E3B3_SESSION_ISOLATA.md).

---

## v60-E3b2-RC3 — Integrity gate SQLite v3 (3 novembre 2026)

- Confermato dall’utente il superamento integrale di `validate.cmd` nella RC2.
- Registrato indipendentemente il numero degli studenti in formazione; rilevata anche la cancellazione dell'ultima riga di `progression_training`.
- Rifiutati i vecchi checkpoint SQLite v3 sperimentali che non hanno il nuovo campo, senza riscriverli. Lo store storico v13 non cambia.
- Aggiunti 3 casi di regressione; **.NET build e test RC3 non eseguiti in questo ambiente**.
- Conservata la directory principale ordinata; nuovo checkpoint in `docs/history`, rapporto in `docs/validation` e ADR 0026.

## v60-E3b2-RC2 — Fix SQLite test, repository root cleanup (3 novembre 2026)

- User Windows gate E3b2-RC1: 377 tests executed, 372 passed, 5 failed in V14SqliteSaveStoreTests; assembly compilation succeeded.
- Keyed round-trip records now compared by stable keys instead of order of SQL rows; genuine sequential training progress remains position-sensitive.
- Corrected missing-school-training-position test to delete actual position 0 and assert one row changed; no save-store behavior changed.
- Relocated 25 checkpoint / release / integrity files from root to docs/history; repaired Markdown links and archived former README indexes.
- Active state, roadmap and handoff refreshed. All existing production C# files, UI, SQLite stores and scripts unchanged.
- Full .NET 10 validation of RC2 pending on Windows.

## v60-E3b2-RC1 — 03/11/2026 (validazione .NET pendente)

- Store `V14SqliteSaveStore` separato, schema SQLite v3 STRICT per tier, scuola, ricerca e militari.
- Scrittura a file temporaneo con transazione; lettura e validazione prima della promozione; backup S1 consistente prima di ogni sostituzione v3.
- Import v13 soltanto su un percorso distinto; rifiuto di file legacy o corrotti alla destinazione.
- Nuovi test Infrastructure per round-trip, reiezione, import e backup; documentazione G1-D9 e ADR 0025.
- Nessuna modifica alla campagna v13, a `GameSession`, `SqliteGameSaveStore` o all’interfaccia.
- v60-E3b-RC1 ha superato i test secondo l’utente; E3b2 non compilata né testata qui.

## 2026-11-03 — v60-E3b-RC1 / G1-D8 (NON VALIDATA)

- Added independent `V14CampaignSnapshot` complete state contract (tiered producers, purchase counters, school/research, military and civilian state) without changing the v13 session or SQLite format.
- Added strict `V14CampaignSnapshots.MigrateFromV13` and validator: original v13 source verified, 16 producer families converted to base, original resources/military/clock preserved, no free unlocks or fabricated population.
- Added seven new Application xUnit tests (written, not run) and proposed SQLite v3 schema/rollback design; ADR 0024 and G1-D8 handoff.
- v59-RC1 remains the last version confirmed green on Windows; v60-E1/E2/E3/E3b remain experimental.

## 2026-11-03 — v60-E3-RC1 / G1-D7 (candidato, test da eseguire)

- E3 isolato nel Core: Scuola parametrica, formazione individuale con coda, qualificati/attivi, riserva della popolazione, annullamento atomico con controllo deposito e rientro civile.
- Catalogo candidato di 8 tecnologie produttive e 3 differite, prerequisiti senza cicli, acquisto volontario e collegamento al modello tier E2.
- DTO checkpoint del solo prototipo con round-trip test; **nessun nuovo schema SQLite né GameSnapshot, nessun cambio UI/scenario v13**.
- Fix preventivo E2: `TieredProductionCatalog.FromCatalog` usa `ToImmutable()` anziché `MoveToImmutable()` per i cataloghi storici con 3 famiglie e builder a capacità 4.
- Aggiunte 2 suite xUnit non eseguite qui; documentazione G1-D7 / ADR 0023. Ultima base validata su Windows: v59-RC1.

## 2026-11-03 — v60-E2-RC1 / G1-D6 (candidato, non validato)

- E2 isolato: tre taglie per le 16 famiglie produttive, personale aggregato per taglia, upgrades, controllo di ricerca sbloccata, acquisti atomici e prezzi E1.
- Adapter alla produzione già esistente per costi di input, output e stoccaggio locale 1/3/10; preservata la simulazione ufficiale v13.
- Nuovi test Core e Application, ADR 0022 e documentazione G1-D6; .NET 10 e gate Windows non ancora eseguiti.

## v60-E1-RC1 — G1-D5 (03/11/2026, non validata)

- Nuovi servizi Core puri per quotare costi progressivi per famiglia e tier, upgrade e acquisti atomici senza mutare la sessione. Coefficienti ancora parametri.
- Aggiunti `ResearchPoints` e `ResearchAccounting` per il saldo ricerca accumulabile senza progetto e spendibile su richiesta, come da ADR 0021 (semantica approvata).
- Nuove suite test Core, non ancora eseguite su Windows. Nessun cambio a ruleset `civil-growth-13`, GameSession, SQLite, GUI o benchmark.
- Roadmap, handoff e dossier G1-D5 aggiornati; prossima tappa E2 con contenuti ancora isolati.

## G1-D4 — Albero tecnologico e strategia della Scuola (solo documentazione, 03/11/2026)

- Documentata proposta di 8 ricerche tecnologiche dei 4 settori e 3 voci differite, 16 famiglie produttive e anti-softlock.
- ADR 0020 proposto; catalogo JSON descrittivo per test statico (non usato dal motore).
- Nessuna modifica a `src/`, `tests/`, `tools/`, `scripts/`, `experiments/` e ruleset `civil-growth-13`. Validazione Windows v59 già confermata dall’utente, non rieseguita.


# v59-G1-D1 — contratto navale proposto (2026-11-03)

- L'utente ha confermato superato integralmente `validate.cmd` sulla **v59-RC1** (benchmark militari e test inclusi). I JSON numerici non sono ancora stati condivisi, quindi M5.3c è validata come automazione ma non chiusa come scelta quantitativa.
- Aggiunta proposta [G1 flotta, esplorazione, conquista](../../design/G1_NAVAL_MISSIONS_DECISION_GATE.md) con N-01…N-07, S-01…S-10, invarianti, dipendenze e gate.
- Aggiunto [ADR 0017 PROPOSTO](../../adr/0017-naval-mission-boundaries-proposta.md) e [checklist G1](../../validation/G1_READY_FOR_IMPLEMENTATION.md).
- Aggiornati stato, roadmap, registro decisioni e handoff; `civil-growth-13` rimane il ruleset corrente. **Nessun sorgente, test, simulatore, script, risorsa, salvataggio o UI è stato modificato.**

---

# v59-RC1 — M5.3c, benchmark militare (2026-11-03)

- La v58-RC5 ha superato `validate.cmd` secondo la conferma dell’utente; numeri di benchmark/DB personali non ancora acquisiti qui.
- Aggiunta modalità di simulazione `--benchmark-military-capacity`: 9 scenari controllati, costi e tempi v13 letti dalle regole effettive.
- Aggiunti 9 test Core parametrici sulla conservazione di abitanti, costi e partizionamento del tempo di addestramento.
- Il validatore registra un nuovo report capacità e il bilancio reale della strategia militare dopo 8 ore, lasciando invariati i vecchi confronti.
- Aggiunto ADR 0016 e aggiornata la documentazione attiva. Nessun cambiamento a gameplay, GUI, SQLite, catalogo. v59-RC1 **da validare su Windows**.

---

## v58-RC5 — 3 novembre 2026: benchmark storici separati dal ruleset corrente

- Riscontro Windows RC4: il validatore raggiunge il controllo industriale `scripts/validate.ps1:102` dopo build e test; la suite xUnit non lo ha interrotto. Il gate complessivo **non è superato**.
- Errore certo nel validatore ereditato dalla v57: il confronto `RulesVersion == civil-growth-9` viene applicato all'esecuzione di `CivilGrowthScenario.Create()`, che dalla v58 restituisce `civil-growth-13`.
- Nuovo parametro del simulatore `--rules historical-v9` (solo `bootstrap`, `industry`, `gold`): esegue la factory storica `CreateMintForBenchmarks()` e conserva i benchmark quantitativi storici con regole v9.
- Il validatore esegue i due percorsi storici Ferro/Oro e il controllo demografico 180 s sotto `historical-v9`, senza cambiare i valori di riferimento.
- Nuove sonde *indipendenti* della campagna attuale `civil-growth-13` per Ferro e Oro: edifici costruiti, addetti, scorte non negative/entro capacità e Municipio presente.
- Nessuna modifica a Core, costo Caserma, reclutamento, capacità, GUI o salvataggi SQLite. Gate RC5 non ancora eseguito con .NET 10: **non dichiararlo verde**. Dettagli: `docs/validation/V58_RC5_VALIDATOR_VERSION_DRIFT.md`.

## v58-RC4 — 3 novembre 2026: correzioni dopo il secondo gate Windows

- Riscontro RC3: **compilazione riuscita, 296/299 test superati, tre fallimenti residui**. I benchmark non sono stati avviati.
- Test Core sul progresso del reclutamento legacy: assert su unico elemento invece di confronto tra collezioni ambigue (`[15]` su entrambi i lati); modello di migrazione invariato.
- Runtime Application: guardia anticipata contro `civil-growth-14` e altre versioni ignote, con test diagnostico più esplicito; versioni conosciute ancora convertibili.
- Desktop Avalonia: visibilità Esercito impostata come proprietà locale, ribadita dopo `Opened` e dopo l'inizializzazione asincrona; nuovi assert per scenari storici e militari.
- **Invariati** Core, schema/lettura/scrittura SQLite, ricette, risorse, prezzi e ruleset.
- Gate Windows RC4 **da eseguire**. [Rapporto](../../validation/V58_RC4_3_REMAINING_TESTS.md).

## v58-RC3 — 3 novembre 2026: correzione quattro regressioni test Windows

- Riscontro RC2: build riuscita; **295/299 test passati**, quattro fallimenti in Core/Application/Desktop.
- Uguaglianza per sequenza di `ImmutableArray` nel test della migrazione individuale.
- Test di incompatibilità aggiornato al ruleset non ancora noto `civil-growth-14`.
- Test UI del Forno coerente con la ricetta in minuscolo esistente, senza alterare la rappresentazione.
- Visibilità della navigazione Esercito legata direttamente al VM, indipendente dalla materializzazione `ListBoxItem` in Avalonia.
- Nessun cambiamento all'economia, al ruleset o al codice SQLite. Verifica Windows della RC3 **pendente**.
- Report: `docs/validation/V58_RC3_299_TESTS.md`.

## v58-RC2 (03/11/2026) — correzione build CS0136
- `src/SimRisorse.Infrastructure/Persistence/SqliteGameSaveStore.cs`, metodo `LoadCore`: rinominate le tre variabili locali `progress` che oscuravano la variabile `int? progress` della crescita civile.
- Nuovi nomi: `trainingProgressSeconds`, `individualProgressSeconds`, `forkProgressSeconds`. Nessun cambiamento a calcoli, schema SQLite, regole, dati o migrazioni.
- Verifica statica dei tre punti effettuata; **build e test xUnit ancora da eseguire su Windows**. Vedere `docs/validation/V58_RC2_CS0136.md`.

## v58-RC1 (03/11/2026) — integrazione non validata
- v57 preservata, recuperata meccanica militare individuale dal fork v41. Nuovo `civil-growth-13`, persistenza/migrazioni e nuovi test (esecuzione pendente).
- Nessuna modifica ai contenuti navali non approvati. Per dettagli vedere `docs/validation/V58_RECONCILIATION.md`.

# Changelog

Questo file registra le modifiche realizzate. Le attività previste appartengono alla [roadmap](../../ROADMAP.md). Non è stata ancora pubblicata una versione della nuova applicazione.

## [Unreleased]

### 2026-10-28 — v57: secondo audit dell'handoff e separazione dello storico

- **Solo documentazione, codice sempre v55 e regole `civil-growth-12` invariate.** Separati `docs/HANDOFF.md` e `docs/ROADMAP.md` (ora solo stato operativo) dagli archivi integrali `HANDOFF_ARCHIVE_V56.md` e `ROADMAP_ARCHIVE_V56.md`.
- Corretti i riferimenti contraddittori a edifici ancora futuri e alle versioni in `GAME_DESIGN.md`, nell'indice e in `SAVE_FORMAT.md` (stato militare obbligatorio anche nella v12).
- Aggiunto [catalogo corrente](../../CATALOG_CURRENT.md), con **16 risorse e 21 strutture**, nomi e ID esatti: `planks` è **Legno lavorato**, anche detto «Tavole» negli studi; `stonecutter` è **Tagliapietra**, talvolta chiamato impropriamente «Cava».
- Riletti e uniformati ingresso, stato, handoff, autorità e roadmap. Gate Windows **287 test attesi** ancora pendente; nessuna scelta navale N-01…N-07 implementata.

### 2026-10-28 — v56: documentazione di passaggio alla nuova chat

- **Nessuna modifica a codice, regole, test, SQLite o UI rispetto alla v55.** Aggiunti `00_START_HERE.md`, `docs/CURRENT_STATE.md`, `docs/NEW_CHAT_HANDOFF.md` e `docs/AUTHORITY_REGISTRY.md` con priorità fra fonti, stato verificato, decisioni approvate, gate pendente e sequenza operativa.
- Allineati gli ingressi di README, HANDOFF, ROADMAP, indice documenti e GAME_DESIGN: gli stati v54 e precedenti rimangono consultabili ma sono dichiarati storici.
- Aggiornata la guida ai salvataggi per indicare chiaramente `civil-growth-12` e il passaggio v11→v12. **287 xUnit ancora attesi; non dichiarati eseguiti**; contratto navale N-01…N-07 ancora non approvato.

### 2026-10-28 — v55: audit statico e protezioni del Municipio

- Verificati file XML Avalonia/MSBuild, collegamenti dei controlli usati dai DesktopTests, 38 pittogrammi 8×8, dipendenze dei progetti e link Markdown: nessun errore statico individuato. Compilazione e test .NET ancora non eseguibili qui.
- Il costruttore `GameScenario` richiede esattamente un esemplare non presidiato di ogni edificio fisso. Esteso il test SQLite di migrazione v11→v12 con una seconda apertura e verifica delle scorte inalterate.
- Nascosti nel dettaglio del Municipio anche i comandi per assegnare/rimuovere addetti; aggiornato il test Desktop. **287 test xUnit attesi**, da validare sul PC Windows. Nessuna variazione di `civil-growth-12`, costi o capacità.
- Preparato [audit v55](../../validation/V55_STATIC_AUDIT.md) e [dossier decisionale navale](../../design/M5_4_DECISION_GATE_V55.md), senza implementare navi o Cantiere.

### 2026-10-28 — v54: Municipio iniziale e capienza distribuita

- **`civil-growth-12`** aggiunge `town-hall` (Municipio) al catalogo come struttura iniziale **unica e fissa**, fuori dai 22 tipi da costruire nella prima campagna. Lo scenario mantiene 8 abitanti, 120 Tavole, 80 Pietra, 30 Cibo e i quattro edifici iniziali preesistenti. Nessuna dotazione viene erogata durante la costruzione o il caricamento: tutte le risorse iniziali sono quelle dello scenario.
- La vecchia capienza invisibile di 1.000 unità per risorsa diventa **+1.000 del Municipio**, con base scenario zero. Ogni produttore conserva +50 per ciascuna risorsa prodotta; Casa piccola +10 Cibo/+10 Pane, Casa grande +25/+25, Caserma +20 Spade; Magazzino ancora +500 per ogni risorsa. Nessun inventario autonomo per gli edifici.
- Core impedisce la costruzione del Municipio (`GameError.FixedBuilding`) e rifiuta snapshot con Municipio assente, duplicato o alterato nel conteggio. Tooltip delle capacità ricostruito sui contributi reali; dettagli edifici con icone delle risorse e funzioni di produzione/stoccaggio indipendenti; costo e costruzione del Municipio nascosti.
- Conversione S1 **v11→v12** con Municipio inizializzato a 1, altri stati e stock immutati; conservati gli upgrade v1…v10 e i cataloghi storici per i benchmark. `military_state` obbligatorio anche in v12; schema SQLite/snapshot v2 invariati, backup al primo salvataggio convertito.
- Nuove regressioni Core/Application, SQLite e UI; **287 test xUnit attesi**, ancora da eseguire in Windows. La futura proposta Cantiere/Flotta slitta a **`civil-growth-13`**. [Protocollo v54](../../validation/V54_TOWN_HALL_DISTRIBUTED_STORAGE.md).

### 2026-10-28 — v53: capienza uniforme e spiegazioni contestuali

- **Regole `civil-growth-11`:** Pane passa da 100 a **1.000 posti base**, come tutte le altre 15 risorse; ogni Magazzino continua ad aggiungere **500 posti a ciascuna**. Nessun prezzo, ricetta, durata o struttura modificati.
- La scomposizione della capienza, lo spazio disponibile e il lotto indivisibile di Pane/Spade/Monete si trovano nel **tooltip del valore Capacità** di ogni riga; eliminato il riquadro permanente e la legenda sotto la tabella Risorse.
- Nel dettaglio di **ogni edificio** compare una riga Capacità per edificio con icona 14 px e testo 12 px ereditando lo stesso `ResourceDetailTemplate` di costi, produzione e consumi. Mostra 8/32 posti abitativi, 2 posti Caserma, +500 unità per risorsa nel Magazzino, oppure assenza di capacità aggiuntiva.
- Conversione esplicita **S1 v10→v11**, senza azzerare scorte, edificio, popolazione, addestramento e tempo. La vecchia factory militare v10 conserva Pane 100, i checkpoint v10 continuano a richiedere `military_state` valida e sono salvaguardati in backup al primo salvataggio v11. SQLite/snapshot **formato 2 invariato**. I contratti navali non approvati slittano alla **futura v12**.
- Aggiornati i test esistenti e aggiunti **4 casi** (2 Infrastructure + 2 Desktop per risoluzione): **280 xUnit attesi**, nessun SDK .NET qui per una conferma esecutiva. [Gate v53](../../validation/V53_STORAGE_UI.md).

### 2026-10-28 — v52: audit della compatibilità navale con SQLite e l'Esercito

- Creato l'[audit d'integrazione](../../design/M5_4_INTEGRATION_AUDIT_V52.md) tramite lettura statica di scenario, snapshot, `GameSession`, `SqliteGameSaveStore`, read model e test esistenti.
- Evidenziati i **due controlli esatti `civil-growth-10`** in `SaveCore`/`LoadCore` da adeguare quando si introdurrà `civil-growth-11`: il nuovo formato deve conservare lo stato militare obbligatorio senza trattarlo come anomalo. Preparati contratti per ID nave, spese atomiche, backup S1 e casi di corruzione che non vanno convertiti silenziosamente.
- **Solo documentazione**: nessuna variazione al gameplay, alle regole v10, SQLite v2, test o GUI rispetto alla v50. **276 xUnit attesi**, v50/v51/v52 ancora senza nuovo gate Windows. La proposta numerica N-01…N-06 della v51 resta da approvare.

### 2026-10-28 — v51: decisione M5.4 Cantiere navale e Flotta, senza nuovo codice

- Nuova [scheda di scelta v51](../../design/M5_4_DECISION_GATE_V51.md), ancorata alla partita `civil-growth-10` e alle regole già approvate per Caserma, Zecca, magazzini e continuità S1. Propone, **senza adottarli**, Cantiere 320 Tavole/220 Pietra dopo Zecca, navi persistenti con ID, acquisto singolo immediato 160 Tavole/120 Monete e capienza di 5 soldati per nave.
- Distinti Cantiere, scafo, flotta e futuro viaggio; controllata la capienza base di 1.000 per Tavole e Monete e calcolati i costi di uno–tre scafi. Le durate teoriche non sono benchmark del gioco reale.
- Predisposte decisioni N-01…N-07, incrementi M5.4a/b/c, test di pagamento atomico, ID, preflight, migrazione e interfaccia. **Restano richieste l'approvazione delle regole navali e la validazione Windows della v50**.
- `src/`, `tests/`, `tools/`, `scripts/`, `validate.cmd`, versioni delle regole e test **invariati**: **276 xUnit attesi**, senza esecuzione .NET in questo ambiente.

### 2026-10-28 — v50: nascondere Esercito nello scenario Foundation

- **Log Windows v49:** compilazione completata, **275/276 test passati**, unico fallimento di `HistoricalFoundationKeepsMilitaryNavigationHidden` alla riga 177: `MilitaryNavigation.IsEffectivelyVisible` è `true` nello scenario senza regole militari.
- Causa individuata nel binding `IsVisible` della voce `ListBoxItem`, che in Avalonia può usare il contesto dell'elemento della lista invece del `MainViewModel`. Rimosso il binding ambiguo e resa esplicita in `MainWindow` la visibilità comune di `MilitaryNavigation` e `MilitaryTab` a partire da `viewModel.HasMilitaryRules`. Il runtime non cambia scenario durante la vita della finestra.
- **Nessuna modifica** a regole, salvataggi, prezzi, cataloghi o test (276 casi); `validate.cmd` sulla v50 resta da eseguire.

### 2026-10-28 — v49: fix CS0136 nella persistenza SQLite

- Il log Windows di `validate.cmd` v48 si arresta durante la build di `SimRisorse.Infrastructure` con **CS0136** alla riga 171 di `SqliteGameSaveStore.cs`. Causa: una variabile di pattern `military` dichiarata nella condizione iniziale di `SaveCore` occupa un ambito che impedisce una seconda dichiarazione `military` nello stesso metodo.
- Rinominata **soltanto la prima variabile** in `militaryCandidate`, con i suoi accessi nella stessa condizione. Nessuna modifica alla transazione, allo schema, ai valori controllati o al comportamento di salvataggio.
- **Test invariati (276 attesi)**: il comando Windows non è arrivato all'esecuzione dei test. Il nuovo pacchetto deve essere ricompilato e verificato con `validate.cmd` prima di proclamare il gate M5.3c superato.

### 2026-10-28 — v48: M5.3c, comandi militari e diagnostica nell'interfaccia

- Tre feature handler (`EnlistRecruits`, `CancelQueuedRecruits`, `DischargeSoldiers`) chiamano atomicamente `GameSession` tramite `GameRuntime`. La loro proiezione aggiorna risorse, forza lavoro e stato militare senza contatori duplicati.
- Nuova scheda **Esercito** visibile solo nello scenario militare: popolazione condivisa, posti Caserma, scorta e costo Spade, reclute in attesa/in formazione, soldati pronti, avanzamento e tempo residuo del gruppo. Ordini da 1/5/10 persone con condizioni di abilitazione e spiegazioni pratiche quando una condizione manca.
- Dettaglio della Caserma: +2 posti di addestramento (non abitativi), testo sulla coda FIFO, nessun lavoratore produttivo, nuovo pittogramma originale 8×8. Aggiornata la descrizione della Forgia all'uso attuale delle Spade.
- Tre nuovi test Application e cinque casi Desktop (due risoluzioni di una prova parametrizzata) verificano transizioni, rimborsi atomici, testo/icone, visibilità nello scenario storico, bottoni e assenza di sovrapposizioni. **276 xUnit attesi, da eseguire su Windows**: SDK .NET non presente nell'ambiente di preparazione. Regole `civil-growth-10`, SQLite v2, backup e benchmark invariati. [Gate v48](../../validation/M5_3C_MILITARY_DESKTOP.md).

### 2026-10-28 — v47: M5.3b, Caserma, popolazione condivisa e salvataggi protetti

- Contratto M-01…M-04 approvato: alloggi e razioni comuni, congedo senza rimborso, annullamento delle sole reclute in coda con rimborso vincolato alla capienza, nessuna perdita permanente per ora.
- Scenario attivo `civil-growth-10` con **20 edifici/16 risorse**, inclusa Caserma; benchmark M5.2c/d congelati sulla factory storica v9. Collegate all'aggregato le transizioni di arruolamento, addestramento, congedo e cancellazione, senza duplicare gli abitanti.
- Stato militare obbligatorio nei checkpoint v10, persistenza SQLite v2 in tabella companion atomica, conversione S1 v9→v10 e backup storico. Rifiutati salvataggi v10 mancanti o corrotti.
- Estesi i test Core/Application/Infrastructure e regressioni di migrazione: **268 xUnit attesi, non eseguiti qui**. La nuova GUI di reclutamento sarà M5.3c. [Gate dettagliato](../../validation/M5_3B_MILITARY_SESSION_SAVE.md).

### 2026-10-28 — v46: M5.3a, primo modello isolato di reclutamento

- L'utente ha confermato **superati i test v45** e ha **approvato il contratto A della Caserma**: 280 Tavole/200 Pietra, Forgia prerequisito, 2 Spade consumate per recluta, due posti per Caserma, gruppo da 60 s, coda FIFO e nessun addetto produttivo.
- Creati `MilitaryTrainingRules`, `MilitaryTrainingState` e il motore puro `MilitaryTraining`, che accetta ordini atomici, controlla la popolazione riservata, non duplica soldati, consuma le Spade una sola volta, rispetta l'attesa dei gruppi successivi e avanza solo a richiesta del chiamante.
- Aggiunti **15 test** in `MilitaryTrainingTests.cs`: parametri, rifiuti atomici, persone occupate, passi 59/60/61 s, 20/30 reclute, capacità aggiunta a metà formazione, replay a blocchi, dati invalidi e limiti numerici. **249 casi xUnit attesi**, non eseguibili qui senza SDK .NET.
- **Incremento deliberatamente isolato:** la partita attiva resta `civil-growth-9`, **19 edifici/16 risorse**; non sono stati modificati `GameSession`, catalogo, app, UI, snapshot o SQLite. Prima della v10 giocabile occorrono integrazione, migrazione S1 e decisioni **M-01…M-04** (alloggi/razioni, congedi, annullamenti e perdite). [Gate e prossime attività](../../validation/M5_3A_MILITARY_CORE.md).

### 2026-10-28 — v45: regressione Desktop sui font dei dettagli edificio

- **Test Windows v44 dell'utente:** compilazione Release riuscita per tutti i nove progetti; **234 test xUnit eseguiti, 232 superati, due falliti**. Entrambi i fallimenti sono i due casi `SidebarNavigationAndSummaryCardsKeepConstructionDetailsInReadingOrder` a **1040×740** e **1240×850**; non è ricomparsa l'eccezione `Brush.get_Transform` dei test v38.
- Individuato un **falso confronto nel test**: il riferimento `costText` proveniva dal Taglialegna, ma dopo aver selezionato la Fonderia ferro il precedente `TextBlock` era stato staccato dall'albero visuale. Il suo font ereditato tornava a `$Default`, mentre il nuovo testo attivo risolveva correttamente il font monospace (`Consolas, DejaVu Sans Mono, ...`).
- In `DesktopTests.cs` il test adesso ricerca **i costi dell'edificio attualmente selezionato**, confrontandone famiglia, dimensione e peso sia con la produzione sia con i consumi del medesimo edificio. **Nessuna modifica UI o stilistica**: regole `civil-growth-9`, 19 edifici, 16 risorse e 234 test invariati. **Gate v45 ancora da eseguire su Windows; benchmark successivi al gate test non eseguiti dal comando v44.**

### 2026-10-28 — v44: correzione dei due errori di compilazione rilevati nel gate v43

- **CS8629** in `ForgeMintCampaignBenchmark.cs`: sostituito l'accesso diretto a `coalAtGoal.Value` con un controllo `?? throw`, riconosciuto dal compilatore anche con nullable e avvisi trattati come errori. Il benchmark continua a rifiutare un profilo privo di scorta Carbone al traguardo, senza cambiare i calcoli.
- **xUnit1051** in `DesktopTests.cs`: passato `TestContext.Current.CancellationToken` a `Task.Run` nel test dei pennelli immutabili. La verifica tra thread resta identica, ma rispetta la cancellazione del framework.
- **Evidenza Windows fornita dall'utente:** `validate.cmd` sulla v43 ha completato il ripristino, ma la compilazione si è fermata con questi **due errori**, uno nel simulatore e uno nei DesktopTests. Nessun test o benchmark è stato quindi eseguito da quel comando. **V44 predisposta; build/test non eseguibili qui senza SDK .NET.** Regole `civil-growth-9`, 19 edifici, 16 risorse, 234 test xUnit attesi e dossier M5.3 non approvato invariati.

### 2026-10-28 — v43: dossier decisionale M5.3 Caserma e reclutamento

- Aggiornata la prefattibilità storica M5.3 alla baseline attiva `civil-growth-9` e alle decisioni già acquisite: Forgia/Spade in v38, Zecca/Monete in v40, **Spade consumate dal futuro comando di arruolamento** senza quantità ancora fissata.
- Preparata la [scheda v43 di scelta](../../design/M5_3_DECISION_GATE_V43.md) con tre alternative verificabili, proposta preferita **280 Tavole/200 Pietra per Caserma, 2 Spade a persona, 2 posti e gruppi di 60 s**, coda FIFO, razioni/alloggi condivisi, regole di congedo e protezione dei salvataggi. **Tutti questi valori sono proposte non approvate** e possono essere cambiati prima del codice.
- Identificati i cambiamenti necessari a `AvailableWorkers`, coda/cronometro e persistenza; matrice di casi atomici, passi frazionari, capienza Spade e conversione S1 candidata `civil-growth-9→-10`. Nessuna nuova risorsa, edificio, comando, interfaccia o conversione è stata implementata: **19 edifici / 16 risorse, 234 xUnit attesi invariati**, gate v42 sempre pendente.

### 2026-10-28 — v42: benchmark da partita iniziale verso Spade/Monete (da validare)

- Preparato `--benchmark-forge-mint-campaign`, **sulle regole v9 effettive e dallo stato iniziale non alterato**. La sequenza costruisce la colonia civile, poi Ferro/Oro, Forgia e Zecca con risorse ottenute solo tramite il gioco; nessuna scorta industriale, abitante, casa o Magazzino viene aggiunto per via diretta.
- Confrontate tre strategie su **2.400 s**: una Miniera di carbone con addetti spostati dalle fonderie, una Miniera con tutte le lavorazioni attive, oppure due Miniere con il costo aggiuntivo. Il benchmark registra gli acquisti reali (2.060/1.400 Tavole/Pietra per una miniera, 2.190/1.490 per due), la scorta di Carbone all'avvio, i blocchi, la popolazione, il tempo dei primi lotti e il raggiungimento di **240 Spade + 360 Monete**.
- Registro delle risorse a ogni secondo, verifiche di capienza, replay identico a passi interi/frazionari con ripristino a metà secondo. Nuovo JSON nel gate `validate.cmd` e [protocollo di interpretazione](../../validation/M5_2D_REAL_START.md). **Non ancora eseguito** per assenza dell'SDK .NET; le durate di arrivo all'obiettivo non sono risultati disponibili. Nessuna modifica al motore, alla GUI, alle regole o ai salvataggi; **234 test xUnit attesi invariati**.

### 2026-10-28 — v41: M5.2c benchmark Carbone (preparato, da validare)

- Aggiunto `--benchmark-forge-mint-coal` nel simulatore ufficiale, usando il catalogo corrente **`civil-growth-9`** senza mutare prezzi, ricette, salvataggi o interfaccia: tre strategie (fasi, doppia miniera, tutte le strutture presidiate) su **1.800 e 3.600 secondi**.
- Ogni scenario compra realmente le strutture industriali, assegna/rilascia addetti, misura tempi per **240 Spade e 360 Monete**, blocchi per Carbone/altre materie/capacità, costi e giacenze; il registro contabile è verificato a ogni secondo. Replay interi e frazionari, con ripresa da snapshot a metà secondo, devono concordare. Gli oracoli **241/122 secondi** sono condizioni da controllare nel Core, **non risultati ancora osservati**.
- Integrato il benchmark nel gate `validate.cmd`, con JSON `forge-mint-coal-benchmark.json` e controlli PowerShell di struttura, bilanci e tempi. **234 casi xUnit attesi invariati**: il benchmark aggiunge verifiche di simulazione, non nuovi test xUnit. Assenza dell'SDK .NET nell'ambiente corrente: build, output quantitativo e fix Avalonia v39 ancora da confermare su Windows. [Protocollo M5.2c](../../validation/M5_2C_COAL_BENCHMARK.md).

### 2026-10-28 — v40: M5.2b Zecca e Monete (da validare)

- Implementata la **Zecca** (210 Tavole/160 Pietra; prerequisito Fonderia d'oro; un addetto; **1 Oro + 1 Carbone → 3 Monete/s**) e la nuova risorsa **Monete**. Priorita' di allocazione **101**, dopo Ferro 90, Oro 91 e Forgia 100; capacita' ordinaria **1.000 + 500 per Magazzino** e lotti non frazionabili. Scenario corrente **`civil-growth-9`, 19 edifici, 16 risorse**.
- Le Monete sono per ora una **scorta**, senza acquisti automatici, reclutamento o comandi navali. La pagina Edifici descrive priorita', consumo e uso futuro; la pagina Risorse spiega quantita', spazio libero, Magazzini e lotto da tre. Icone pixel originali aggiunte per Zecca e Monete.
- Migrazione S1 esplicita **v8→v9**: Zecca nuova a zero e Monete inizialmente zero; i checkpoint v8 con Monete preesistenti sono rifiutati. Conservati slot civile, SQLite/snapshot v2, backup originario, lavoratori, tempo e frazioni. Creato `CreateForgeForBenchmarks()` per mantenere immutata la versione storica **v8**; v7, v6 e baseline M4 restano disponibili. Validatore aggiornato al nuovo identificatore v9.
- Predisposti **12 casi xUnit aggiuntivi** (9 Application, 1 Infrastructure, 1 Desktop e 1 dato parametrizzato della conversione) per un totale di **234 attesi**. Comprendono ricetta/primo lotto, capacita' 997/998/999, contesa del Carbone, spesa non monetaria delle costruzioni ordinarie, replay frazionario, rifiuto di vecchie Monete, SQLite con Spade e interfaccia. **Non compilati ne' eseguiti** in questo ambiente senza SDK .NET.
- **Gate Windows ancora pendente:** la v39 ha corretto la probabile causa comune dei 33/53 DesktopTests falliti in v38, ma la correzione non e' stata ancora verificata. L'avanzamento della Zecca e' un incremento separato preparato su richiesta dell'utente, non una dichiarazione di stabilita'. [Controlli M5.2b](../../validation/M5_2B_MINT.md).

### 2026-10-28 — v39: correzione del crash comune ai DesktopTests

- Esaminato il log Windows della v38: **53 DesktopTests eseguiti, 20 superati e 33 falliti**, tutti per la medesima `InvalidOperationException` di Avalonia durante il rendering di `TextBlock` (`Brush.get_Transform`: oggetto creato da un thread diverso). Il restore e la compilazione delle dipendenze Desktop sono riusciti; **il gate non e' passato**.
- Individuata la causa nel codice applicativo: `ResourceRow` conservava in campi `static` due `SolidColorBrush` **mutabili** per il saldo positivo/negativo. I test headless isolano ogni caso con un Dispatcher diverso; la palette condivisa veniva usata da thread differenti. Sostituiti con `ImmutableSolidColorBrush`, mantenendo invariati i due colori, le associazioni dati, i salvataggi e le regole economiche.
- Aggiunto un test Desktop senza interfaccia che verifica l'immutabilita' **di entrambi** i pennelli e la loro lettura su un thread differente. **222 casi xUnit attesi** nella soluzione, incluso il nuovo test; **non eseguiti in questo ambiente** (SDK .NET assente). La riuscita della v39, e quindi della Forgia v38, resta da verificare col gate Windows.
- Per una riprova rapida: `dotnet test tests/SimRisorse.Desktop.Tests/SimRisorse.Desktop.Tests.csproj -c Release --logger "console;verbosity=normal"`; solo dopo un esito positivo lanciare `validate.cmd`.

### 2026-10-28 — v38: M5.2a Forgia e Spade (da validare)

- Attivata **Forgia** nella campagna: costruzione a **240 Tavole + 180 Pietra**, prerequisito Fonderia di ferro, un posto di lavoro, **1 Ferro + 1 Carbone → 2 Spade per addetto/s**. Le Spade sono per ora una scorta, non ancora consumata da soldati inesistenti.
- Nuova versione `civil-growth-8` con **18 edifici e 15 risorse**, capacità delle Spade di **1.000 base + 500 per Magazzino**. Confermata priorità di allocazione delle fonderie **90/91** davanti alla Forgia **100**, con lotti interi e senza cambiare il Core economico.
- Conversione esplicita S1 **v7→v8**: Forgia in stato 0/0, Spade a zero; rifiutate scorte di Spade dichiarate sotto regole precedenti. SQLite/snapshot v2, tempo, progressi, edifici, lavoratori, scorte e backup precedente invariati.
- Icone pixel dedicate, descrizione delle cause di blocco in Edifici e spiegazione del lotto da 2 nella Risorsa Spade. Catalogo industriale v7 congelato per il benchmark M5.1c e per l'esperimento Forgia/Zecca, che resta isolato.
- Aggiunti **9 casi xUnit attesi** (sei Application, uno Infrastructure, uno Desktop, uno nuovo dato della teoria di migrazione): **221 totali attesi**, **non eseguiti** per assenza del .NET SDK. Errori `DesktopTests` segnalati dall'utente in precedenza **ancora non identificati** senza log. La Zecca/Monete è rimandata a M5.2b dopo un gate positivo.

### 2026-10-28 — v37: spiegazione dei depositi e robustezza test Desktop

- Chiarito il significato degli edifici: il **Deposito alimentare produce Cibo dal Pesce** e non conserva Pane; il **Magazzino** aumenta di **500** unità la capacità di *tutte* le risorse. Descrizioni di Forno, Deposito alimentare e Magazzino aggiornate senza cambiare ricette, costi o limiti.
- Il messaggio «Spazio insufficiente» nel dettaglio Edifici indica ora, per ogni risorsa bloccata, **quantità precisa, capacità, spazio libero e dimensione del lotto per addetto**, consigliando il Magazzino. Per Pane evidenzia anche la differenza dal Deposito alimentare. Il calcolo conserva la precisione del simulatore: non usa l'arrotondamento della colonna Quantità.
- Quando si seleziona una riga della tabella Risorse, un riquadro espone **scorte, spazio libero e capienza iniziale + contributo dei Magazzini**; per Pane indica il lotto da 3 del Forno. Sempre visibile la legenda Magazzino/Deposito alimentare.
- Anche il riepilogo della filiera Pane nella sidebar suggerisce ora il **Magazzino** quando un lotto non entra nel deposito; aggiornate le relative aspettative dei test Desktop esistenti.
- Aggiunto un caso xUnit sul Pane a 99/100, informazione di stoccaggio e costruzione di un Magazzino; ampliato il test di selezione Risorse con verifica del riquadro. Resa meno dipendente dal nome interno del template Avalonia la ricerca dello sfondo ocra nel test v36. **212 casi xUnit attesi**, ancora da eseguire in Windows; l'utente ha segnalato errori nei `DesktopTests` ma il loro log specifico non è stato fornito, quindi non è possibile attribuirne la causa con certezza.
- La base giocabile resta **`civil-growth-7`** con **17 edifici / 14 risorse**; salvataggi, capacità iniziale del Pane (100), regole di produzione e contratto M5.2 A rimangono invariati.

### 2026-10-28 — v36: scrollbar separate dalle colonne e selezione ocra nelle Risorse

- Le righe degli elenchi **Risorse** ed **Edifici** riservano un margine destro di 20 px alla scrollbar verticale. Anche l'evidenziazione della riga si ferma prima di quel margine; le intestazioni sono arretrate per evitare che l'ultima colonna finisca dietro alla barra.
- La tabella Risorse passa da `ScrollViewer` + `ItemsControl` a un `ListBox` con scorrimento interno: al clic, la risorsa resta selezionata in **ocra** (`#E6D5AC`), usando lo stesso stile dell'elenco Edifici. Il ViewModel conserva la riga selezionata durante gli aggiornamenti delle quantità.
- Due nuovi casi xUnit Desktop, a **1040×740** e **1240×850**, controllano geometria delle due liste, selezione al clic, colore e persistenza della selezione dopo l'avanzamento: **211 casi attesi**, da compilare e verificare su Windows; non si attribuisce loro l'esito dei test v33 dichiarati superati.
- **Decisione ricevuta M5.2:** approvata l'opzione A completa (prezzi e ricette Forgia/Zecca, un addetto per struttura, destinazione futura di Spade/Monete, priorità 100/101, capacità ordinaria e lotti interi). Registrata nel [dossier](../../design/M5_2_FORGE_MINT_PREFEASIBILITY.md); **Forgia e Zecca non sono ancora implementate**, né sono stati decisi i quantitativi di reclutamento e acquisto navi. Nessun cambiamento a simulazione, catalogo e salvataggi in v36.

### 2026-10-28 — v35: stima riempimento e uniformità delle risorse nel Desktop

- La tabella **Risorse** aggiunge **Si riempie tra** in ore:minuti:secondi, calcolata da spazio residuo e saldo netto previsto per il prossimo secondo. Se già piena mostra `00:00:00`; se il saldo è nullo/negativo o la capacità non è positiva mostra `—`. È una stima a tassi costanti, non una previsione degli eventi futuri.
- Allineate a destra intestazioni e celle numeriche di **Quantità, Capacità, Produzione, Consumo e Saldo**, insieme al nuovo tempo; mantenuta la lettura alla larghezza minima di 1040 px con colonne più compatte.
- **Solo la colonna Quantità della tabella** visualizza unità intere (arrotondamento a metà verso l'alto); le card mantengono fino a tre decimali e il gioco conserva tutti i decimali per simulazione, confronti e salvataggi.
- Nel dettaglio **Edifici** i consumi per addetto/secondo sono un elenco di righe con icona, carattere e dimensioni identici a costi e produzioni.
- Aggiunto un test xUnit per quantità e stime (saldo positivo, nullo, negativo, deposito pieno e tempi oltre 24 ore); estesi i test UI per allineamento delle colonne e dettagli dei consumi alle due risoluzioni. **209 casi attesi**, nuova suite da eseguire su Windows; l'ultimo gate dichiarato superato dall'utente resta la v33 (208 casi attesi). Nessuna modifica a regole, catalogo, SQLite o all'esperimento M5.2.

### 2026-10-28 — v34: prove M5.2 sui limiti di deposito e sulle priorità

- Conferma dell'utente: i test della **v33 sono passati**. L'esito è riferito dall'utente, senza allegato del log/TRX; i conteggi **206/206** nel rapporto storico restano attribuiti al gate precedente e i **208** della v33 sono il totale atteso dei casi xUnit.
- Esteso **solo** l'[esperimento M5.2](../../../experiments/forge-mint-probe/README.md), ancora esterno alla soluzione e al validatore, con tre guardie per ciascuna delle ipotesi prezzo A/B: deposito quasi pieno, riempimento esatto con lotti interi e contesa sul Carbone con precedenza alle fonderie.
- I nuovi casi impiegano `GameSession.Construct`, `AssignWorkers`, `ForecastEconomy` e `Advance`, controllano blocchi/consumi e confrontano ciascuno il replay 1 s con 250+750 ms, incluse le immagini dello stato serializzato.
- **Ancora da eseguire in .NET** il nuovo esperimento; non sono cambiate le regole giocabili, il catalogo, le partite, il motore, la GUI o i test xUnit della v33. Prezzi, ricette e destinazioni di Spade/Monete restano da approvare.

### 2026-10-27 — v33: elenco Risorse in ordine alfabetico

- La tabella **Risorse** di Avalonia è ora ordinata per **nome visibile in italiano**, ignorando le differenze tra maiuscole e minuscole e con ID come criterio secondario deterministico. L'ordinamento viene definito all'inizializzazione, senza riordinamenti durante gli aggiornamenti delle quantità.
- I tre indicatori delle scorte sulla schermata principale conservano l'ordine precedente. Catalogo, priorità di produzione, salvataggi e simulatore non cambiano.
- Aggiunti due casi xUnit Desktop (scenario base e civile): ordine alfabetico, proiezione economica immutata, ordine delle card, identità delle righe e aggiornamento dopo l'avanzamento.
- **Alla preparazione:** non era disponibile il compilatore .NET. **Aggiornamento 28/10:** l'utente ha confermato «test passati» sulla v33; il relativo log non è stato acquisito qui. ForgeMintProbe resta esterno al gate ordinario.

### 2026-10-27 — v32: prova isolata M5.2, senza nuove regole attive

- Creato il [progetto sperimentale ForgeMintProbe](../../../experiments/forge-mint-probe/README.md), fuori dalla soluzione e dal validatore: costruisce in memoria uno scenario derivato da `civil-growth-7`, aggiungendo **solo in ipotesi** Forgia/Zecca, Spade/Monete e priorità 100/101.
- Sei profili indipendenti **A/B × (produzione a fasi, due miniere parallele, tutte le macchine presidiate con una miniera)**; acquistano le strutture con `GameSession`, riconciliano consumi e output secondo per secondo, confrontano replay da 1 s e 250+750 ms e controllano gli oracoli 241/122 s, scorte e costi.
- Conservati `src/`, `tests/`, `tools/`, `scripts/`, progetto principale e salvataggi senza modifiche. Allineati dossier M5.2, roadmap, handoff, indice e guida di sviluppo; nessun parametro M5.2 considerato approvato.
- **Stato:** calcoli numerici controllati separatamente, ma **nessuna compilazione .NET né esecuzione dell'esperimento in questo ambiente**. Anche M5.1c rimane in attesa di `validate.cmd` su Windows. L'esperimento non costituisce il superamento di un gate.

### 2026-10-27 — v31: contratto preliminare M5.2, senza cambiamenti al gioco

- Ampliato il [dossier Forgia/Zecca](../../design/M5_2_FORGE_MINT_PREFEASIBILITY.md) senza approvare ricette o prezzi: modello candidate A/B, significato Spade/Monete, priorità dei quattro trasformatori e capienza a lotti indivisibili.
- Confrontate aritmeticamente due politiche sullo **stesso obiettivo di 240 Spade + 360 Monete** con minerali già pronti: **241 s** con una miniera a fasi e **122 s** con due miniere sovrapposte, a condizioni sintetiche ideali e con priorità proposte. Sono **calcoli teorici**, non output eseguiti da .NET e non tempi dalla nuova partita.
- Aggiunti i casi di accettazione F8–F10: deposito quasi pieno, priorità deterministica, conversioni S1. Esplicitata la differenza tra rilascio congiunto `civil-growth-8` e rilasci separati `-8/-9` per evitare di cambiare catalogo lasciando invariato un ID di regole già persistito.
- Aggiornati la scheda D0, la roadmap, l'handoff e il README. **Solo documenti**: il codice e il validatore della v30 sono invariati; M5.1c resta da verificare su Windows e M5.2 resta da approvare.

### 2026-10-27 — Consolidamento locale e M5.1c (codice preparato)

- Assunta come nuova baseline la revisione locale `civil-growth-7` dell'utente, **senza riscrivere** Core, Desktop, test, prezzi o salvataggi. Conservati gli esiti Windows **206/206 test** come *evidenza riferita dai documenti dell'archivio*, non come nuova esecuzione.
- Aggiunto al simulatore il benchmark `--benchmark-industry-scale`: **cinque profili sintetici** con una/due Miniere di carbone, fonderie di Ferro e Oro in concorrenza, 180 secondi ciascuno. Acquisti e assegnazioni reali, riconciliazione delle scorte ogni secondo, confronto di replay interi/frazionari e verifica degli snapshot finali.
- Esteso `scripts/validate.ps1` con un quinto benchmark, controllo dei risultati attesi e `industry-scale-benchmark.json`. [Protocollo, assunzioni e tabella teorica](../../validation/M5_1C_INDUSTRY_SCALE.md).
- Corrette le parti normative obsolete di roadmap, handoff e indice: D0 e M5.1a/b non risultano più «da iniziare». **Nessuna build o esecuzione .NET effettuata in questo ambiente**: M5.1c rimane da validare su Windows.

### 2026-10-27 — Riordino della documentazione e screenshot nel README

- README riscritto con screenshot dell'applicazione subito dopo il titolo, avvio rapido, funzionalità presenti e limiti della campagna. PNG copiato integralmente dal render 1240×850 dell'ultimo gate in `docs/screenshots/colonia.png`, con provenienza e istruzioni di aggiornamento.
- Nuovo indice `docs/README.md`, handoff corrente sintetico e archivio dei punti di ripresa precedenti. Guida grafica allineata a sidebar da 240 px, card, icone, costi/produzione e hover ocra.
- Allineati roadmap, sviluppo, architettura e decisioni ai **206 test** dell'ultimo gate. Nuovo rapporto `docs/validation/DESKTOP_UI.md`; i conteggi e risultati dei gate storici restano conservati nei rispettivi rapporti. Nessuna modifica al codice applicativo o alle regole.
- Verificati **62 file Markdown e 468 collegamenti locali**, incluse le ancore, senza errori. Controllati formato, dimensioni, identità SHA-256 e inclusione in Git dello screenshot.

### 2026-10-27 — Hover ocra dei pulsanti

- Hover dei pulsanti primari e secondari nello stesso ocra della struttura selezionata (`#E6D5AC`), con testo scuro. Colore condiviso nel tema e applicato anche al contenuto del pulsante Fluent; disabilitazione grigia e geometria stabile preservate.
- `validate.cmd` completo: **206/206 test**, zero avvisi/errori, smoke nativi superati. Verificati hover di Costruisci e Salva, contrasto del testo e passaggio a disabilitato con puntatore fermo; run `899f6ff3e8ff41288330c3d26b4c4ea7`.

### 2026-10-27 — Dettaglio strutture e navigazione compatta

- Ulteriore rifinitura richiesta: icona pixel degli abitanti, sidebar ampliata ancora a 240 pixel, produzione con icone delle risorse e medesimo testo normale da 12 px dei costi.
- Costi di costruzione in elenco subito dopo la descrizione, seguiti da produzione per addetto/secondo ed eventuali consumi; i costi si aggiornano con l'ordine +1/+5/+10.
- Colonna sinistra da 185 a 215 pixel con menu Edifici/Risorse; abitanti, posti e lavoratori liberi in una card accanto alle scorte, visibile in entrambe le pagine. Testata più bassa per lasciare spazio agli elenchi.
- Aggiunte verifiche di ordine dei contenuti, costi multipli, click sui menu, aggiornamento della popolazione e barre contenute nelle card a 1040×740 e 1240×850. Regole `civil-growth-7` e salvataggi invariati.
- `validate.cmd` completo superato: **206/206 test**, zero avvisi/errori, simulazioni, benchmark e smoke nativi per entrambi gli scenari. Render controllati in formato compatto e standard; ultima run dopo le rifiniture `a6f36b25cada4bdfaf4d880a757cc788`.

### 2026-10-27 — M5.1b: Oro e carbone condiviso

- L'utente ha scelto la configurazione A per Oro, separatamente dal Ferro e dalla continuità S1 già acquisiti. Aggiunti Miniera d'oro, Fonderia d'oro, due risorse e quattro icone; **`civil-growth-7`, 17 edifici / 14 risorse**. Ferro prioritario con carbone scarso; seconda miniera capace di ripristinare la produzione dell'Oro.
- Conversione v6→v7 con ferro, lavoratori, Magazzini e frazioni conservati, risorse/edifici nuovi a zero e backup dell'originale alla prima scrittura. Formato SQLite v2 invariato; casi di ripresa e scorte incompatibili verificati.
- Strategia `gold` da nuova partita: primo Oro a **1.502 s**, Ferro 401 e Carbone 402; a otto ore entrambi i metalli a 1.000 e popolazione 48. `validate.cmd` completo: **204/204 test**, zero avvisi/errori, 16 cronologie più crescita, quattro benchmark, 17 render e smoke nativi di entrambi gli scenari. [Report](../../validation/M5_1B_GOLD.md). Forgia e Zecca restano future.

### 2026-10-27 — M4 chiusa e prima filiera giocabile ferro/carbone

- Registrate le risposte dell'utente: base grafica monospace accettata, demolizione rinviata; vittoria per possesso dei 22 tipi con Castello conclusivo, sbloccato da una conquista obbligatoria. Scelta S1 e configurazione A per la sola M5.1a.
- Integrati Miniera ferro, Miniera carbone e Fonderia ferro, con tre risorse, capacità e sei icone originali. Regole **`civil-growth-6`**, 15 edifici / 12 risorse; conversione concatenata dai checkpoint precedenti, preservando frazioni, scorte e backup originale nel formato SQLite v2.
- Nuova strategia simulata `industry`, primo ferro a 1.102 s da nuova partita e deposito pieno/48 abitanti a otto ore. Cataloghi benchmark v4/v5 e prova A/B storica preservati; aggiunti test di produzione, capienza, conversione, ripresa SQLite e flussi/render Desktop.
- **Verificato:** `validate.cmd` completo, **191/191 test**, zero avvisi/errori, 14 cronologie e prova accelerata, quattro benchmark, 13 render e smoke nativi per entrambi gli scenari. [Misure M5.1a](../../validation/M5_1A_IRON.md) e [ADR 0014](../../adr/0014-first-iron-industry-and-civil-save-upgrade.md). Oro e contenuti successivi non inclusi.

### 2026-10-27 — Tipografia monospace per l'interfaccia retro

- Applicata la richiesta dell'utente di usare un carattere monospace o pixel: **Consolas** condiviso da titoli, cifre, tabelle e controlli, con fallback DejaVu Sans Mono, Liberation Mono e Courier New. La famiglia è definita una volta nel tema Avalonia; rimosso il riferimento locale a Segoe UI dalla finestra.
- Nessun nuovo pacchetto o asset esterno. `validate.cmd` completo superato: build senza avvisi/errori, 177/177 test (incluse verifiche di prerequisiti visibili e geometria hover), simulazioni e smoke nativi di entrambi gli scenari. Ispezionati i render civili a 1040×740 e 1240×850, inclusi tabella Risorse e stato avanzato del Pane. Run `f87ccb76a0c245bd95c4db8589d0a194`.

### 2026-10-27 — Prova industriale A/B separata dal gioco

- Aggiunto [IndustryProbe](../../../experiments/industry-probe/README.md), eseguibile senza nuove dipendenze esterne, fuori dalla soluzione di produzione. Usa Core/Application e una colonia sintetica per misurare ipotesi A/B su ferro e oro, con acquisti in blocco o progressivi.
- Eseguiti otto confronti con riconciliazione di 3.601 secondi ciascuno, ripetuti con passi da 1 s e 250+750 ms; rapporti e snapshot identici, saturazione e ripresa dopo espansione pagata del deposito verificate. Primo ferro progressivo A/B: **452/677 s**; primo oro: **852/1.277 s**. La politica progressiva evita il Magazzino intermedio del profilo B con budget intero.
- Build sperimentale senza avvisi/errori; nuovo `validate.cmd` completo superato con **177/177 test**, simulazioni e smoke nativi per entrambi gli scenari. Ipotesi economiche, versioni di regole sperimentali e capacità restano limitate alla prova: nessuna nuova regola nel desktop o migrazione dei salvataggi.

### 2026-10-27 — D0.1: scelto il catalogo della prima campagna

- Preparata la [scheda concreta della prima campagna](../../design/FIRST_CAMPAIGN_PROPOSAL.md) con corrispondenza ai 43 riferimenti, ID presenti/candidati, percorso funzionale e sette casi proposti di vittoria/non vittoria.
- **L'utente ha scelto i 22 tipi proposti:** i 12 presenti più cinque tipi di estrazione/fusione, Forgia, Zecca, Caserma, Cantiere e Castello. Gli altri 21 riferimenti sono fuori dalla prima campagna. Registrata D0.1 nel game design, dossier D0 e handoff; vittoria, isole, costi, ricette e salvataggi restano decisioni separate. Nessun contenuto M5 implementato.

### 2026-10-27 — Consolidamento dei risultati M4 dopo il gate Windows

- Eseguito `scripts/report-m4.ps1` sui quattro benchmark reali e confrontata l'uscita con una lettura delle copie isolate. Conservati localmente i cinque input JSON con impronte SHA-256 e il rapporto generato, senza modificare le simulazioni.
- Ispezionati i nove render e i conteggi dei quattro TRX. Creato il [rapporto delle misure M4](../../validation/M4_MEASURED_RESULTS.md): confronto abitazioni v4/v5, costi e carenze Pane a 220/320 abitanti, revisione visiva e limiti della verifica. Aggiornati i punti di ripresa; approvazione estetica, Carne/demolizione e decisioni M5 restano aperte.

### 2026-10-27 — Correzione del gate Windows: palette PixelGlyph

- Sostituiti i pennelli statici mutabili di `PixelGlyph` con `ImmutableSolidColorBrush`, vincolando la palette a `IImmutableBrush`. Il controllo del catalogo può inizializzare il tipo fuori dal thread grafico: i pennelli condivisi ora si possono usare nel rendering senza l'eccezione Avalonia di accesso da un thread diverso, che causava 21 errori Desktop.
- **Verificato con `.\validate.cmd` su Windows**: build Release con zero avvisi/errori, **177/177 test superati**, controlli cataloghi, simulazioni e quattro benchmark completati, nove render generati e prove native di salvataggio/caricamento/ripresa superate per entrambi gli scenari. Report locale `.local-cache/m4/validation.json`, esecuzione `98d961210607483a8afe8d708039efab`. Nessun cambiamento alle regole o ai salvataggi.

### 2026-10-27 — v29 / M4.15: studio navale M5.4 (solo progettazione)

- Creato il [dossier M5.4 su Cantiere navale, flotta e trasporto](../../design/M5_4_NAVAL_PREFEASIBILITY.md), distinguendo quanto appare nei diagrammi da ciò che resta da decidere: costo del Cantiere e delle navi, uso delle Monete, capacità, equipaggio, riutilizzo, ritorni e commercio coloniale.
- Confrontati tre modelli di nave (monouso, entità persistente, capacità astratta); formalizzati i controlli sul **primo viaggio raggiungibile**, gli acquisti atomici, l'identità delle navi, le future prenotazioni senza doppio impiego e i test di salvataggio. Nessuna proposta o costo è stato approvato.
- Roadmap, scheda D0, game design, handoff, README e verifica M4 allineati: **M5.4a/b/c restano futuri**, distinti dalle missioni M5.5 e dalla conquista M5.6. Nessuna modifica a codice, test, benchmark, UI, risorse, regole `civil-growth-5` o SQLite. Gate confermato dall'utente fino a **v24** (174 xUnit attesi); v25–v29 **pendenti su Windows** (177 xUnit attesi).

### 2026-10-27 — v28 / M4.14: scheda operativa D0 e correzione delle baseline M5

- Creata la [scheda decisionale D0](../../design/D0_DECISION_BOARD.md): otto domande, preferenze **non approvate**, ordine delle dipendenze, verifiche richieste, politica S1/S2 di salvataggio e criteri per iniziare M5.1a.
- Corretti gli studi [M5.1](../../design/M5_1_INDUSTRY_PREFEASIBILITY.md), [M5.2](../../design/M5_2_FORGE_MINT_PREFEASIBILITY.md) e [M5.3](../../design/M5_3_ARMY_PREFEASIBILITY.md), che in vari punti citavano erroneamente `civil-growth-4` come base eseguibile: la versione **corrente è `civil-growth-5`**; l'eventuale `civil-growth-6` è **solo una proposta** subordinata alla scelta S1. I test futuri devono includere anche la conversione da `-5`.
- Allineati dossier D0, roadmap, handoff, README e verifica M4. **Nessuna modifica** a motore, GUI, test, benchmark, cataloghi, SQLite o salvataggi. Ultimo gate confermato dall'utente: **v24** (174 xUnit attesi); v25–v28 **pendenti** (177 xUnit attesi), senza SDK .NET/PowerShell in questo ambiente.

### 2026-10-27 — v27 / M4.13: audit degli incentivi abitativi e allineamento del gate D0

- Redatto l'[audit matematico delle abitazioni](../../scenarios/HOUSING_INCENTIVE_AUDIT.md) a partire da `HousingGrowthRules`, `CivilGrowthScenario` e dalla razione `PopulationPlanner`, senza modificare il gioco. Il limite **2×** richiede sette unità di bonus equivalenti: le quattro combinazioni a minimo costo abitativo spendono tutte **560 legno lavorato e 280 pietra**, ma offrono **64/80/96/112 posti**.
- Quantificati per ciascuna combinazione Cibo e Pane necessari a piena capienza, numero nominale di filiere e lavoratori alimentari; distinta la convenienza **per posto** dalla spesa di sblocco della filiera Pane e dai rischi di carenza. A 112 residenti una filiera Cibo da 1/s non copre più il consumo continuo.
- Aggiornato il [dossier D0](../../design/PRE_M5_DECISION_GATE.md): stato reale v24–v27, versione attiva `civil-growth-5` e tetti **ideali** di 968 abitanti in 8 ore / 3.848 in 32 ore a bonus 2× fin dall'inizio, anziché applicare impropriamente i precedenti limiti a 60 s dell'algoritmo storico.
- Roadmap, handoff e documenti M4 allineati. **Nessun cambiamento al Core, alla GUI 8-bit, ai benchmark, agli schemi SQLite o ai salvataggi** e **nessun nuovo test xUnit**: **177 casi attesi** come per v25/v26, non eseguiti qui. v24 resta l'ultimo gate riferito superato dall'utente; D0 e i ritocchi economici restano decisioni aperte.

### 2026-10-27 — v26 / M4.12: rapporto quantitativo M4, senza toccare le regole

- **v25/M4.11 resta da verificare su Windows** (177 test xUnit attesi). L'utente non può eseguire ora i test: nessun ulteriore cambiamento alla GUI non ancora osservata o alle regole di gioco.
- Nuovo `scripts/report-m4.ps1`: legge i **quattro JSON** prodotti dalla validazione e produce una sintesi Markdown di tempi 48/100/136 abitanti con/senza bonus, confronti del Pane a 220/320, carenze, scorte, produzione, consumo e investimenti aggiuntivi.
- Controlla versione dello scenario, durata, ID delle strategie, duplicazioni e **riconciliazione di ogni secondo**; segnala assenza di report anziché usare valori teorici come risultati osservati. Non avvia .NET e non legge o modifica salvataggi personali.
- Di default stampa il resoconto. Un file si crea soltanto con `-OutputPath`, senza sovrascrivere file esistenti salvo `-Force` esplicito. Lo script è codificato UTF-8 con BOM per PowerShell Windows.
- Creato il [manuale di lettura dei benchmark](../../validation/M4_BENCHMARK_READER.md), aggiornati README, roadmap, handoff e report M4. **Nessun test Windows/PowerShell eseguito qui**, né nuovo test xUnit; **177 casi previsti** come in v25. Nessuna meccanica M5 è approvata.

### 2026-10-27 — v25 / M4.11: prima schermata retro 8-bit (gate Windows in attesa)

- Il **gate v24/M4.10 è riferito superato dall'utente** (174 casi xUnit attesi, TRX/benchmark grezzi non allegati). Le regole `civil-growth-5` restano il riferimento della nuova GUI.
- Introdotto `PixelGlyph`, controllo Avalonia autonomo con **21 sprite vettoriali a celle 8×8** progettati per 12 edifici e 9 risorse. Identificatori di catalogo → disegni originali, senza nuovi pacchetti né asset esterni.
- Integrati i pittogrammi nell'elenco strutture, nelle schede delle scorte e nella tabella Risorse. Prima palette retro blu notte/crema/azzurro e pannelli con bordi netti; numeri, testi, diagnosi e comandi restano visibili.
- Mantenuto lo spessore fisso del bordo del pulsante **Costruisci** per evitare la regressione dell'hover; mantenuta la diagnosi dei prerequisiti sopra ai comandi senza scorrimento.
- Aggiunti tre casi Desktop attesi: copertura di tutti gli ID civili e verifica delle icone e diagnosi alle due risoluzioni. **177 test xUnit attesi**, non eseguiti qui senza SDK .NET; necessari `validate.cmd` e controllo visivo dei PNG.
- **Nessuna modifica a prezzi, ricette, regole, salvataggi, backup o benchmark.** È un prototipo da valutare (D0.7), non la GUI finale.

### 2026-10-27 — v24 / M4.10: accelerazione abitativa `civil-growth-5` (nuovo gate Windows in attesa)

- Ogni Casa piccola oltre la prima aggiunge **15%** alla velocità di arrivo di un residente; ogni Casa grande aggiunge **30%**. Bonus cumulativi e limitati a **2×** (60→30 secondi a velocità massima). Nessun bonus per la sola Casa piccola iniziale; cibo, Pane necessario e capienza continuano a vincolare gli arrivi.
- Introdotto accumulatore deterministico in ventesimi di secondo: cambi di velocità, pause, carenze e passi temporali suddivisi non perdono la frazione. Forecast e GUI mostrano velocità, progresso e stima dei secondi all'arrivo.
- Conversione dichiarata `civil-growth-4 → civil-growth-5`, con backup dell'originale; formato snapshot/SQLite **v2** invariato. La tabella opzionale `population_growth_fraction` salva il resto: vecchi file v2 senza tabella restano leggibili con frazione zero.
- Conservati i benchmark quantitativi M4 a regole `civil-growth-4`, separati dal nuovo `--benchmark-housing-accelerated`. Aggiunti **7 test xUnit** in Application, Infrastructure e Desktop (totale **174 attesi**) e una prova di crescita nel validatore Windows.
- **Codice implementato ma non compilato né verificato con .NET in questo ambiente**. Il gate v23 è riferito superato dall'utente; ricette, costi, catalogo, decisioni M5 ancora aperte e futuro stile pixel art restano invariati.


### 2026-10-27 — Checkpoint v23: hover senza salto di layout e studio M5.3

- L'utente conferma **«test passati» per v22** (GUI compatta, 166 casi xUnit previsti); i report grezzi non sono disponibili in questa sessione. Ha segnalato che passando su **Costruisci** gli elementi precedenti si spostano verso l'alto.
- Causa individuata nel codice: `Button.primary.build:pointerover` applicava `BorderThickness=2`, mentre il disabilitato aveva `1`. Il diverso bordo variava l'ingombro del controllo all'attivazione dell'hover. Ora `Button.primary.build` mantiene **BorderThickness=1 in ogni stato**; hover/pressione/disabilitato modificano colore e bordo ma non spessore. Aggiunto un nome al testo del costo per verificarne la posizione.
- Rafforzato il test Desktop: confronto di posizione e dimensioni di **selettore quantità, costo e pulsante** prima/dopo hover e dopo esaurimento dei materiali, sia a **1040×740** sia a **1240×850**. Il precedente caso singolo è ora una teoria da due: **167 casi xUnit complessivi attesi** (+1 rispetto a v22), **non eseguiti qui** per assenza di .NET.
- Preparato il [dossier M5.3](../../design/M5_3_ARMY_PREFEASIBILITY.md): Caserma, reclute, soldati, conservazione della popolazione, alternative di addestramento, test e decisioni D0.4/D0.8. **Solo prefattibilità:** non sono approvati costi, tempi o perdite e non è stato implementato alcun contenuto militare. Regole `civil-growth-4`, salvataggi e benchmark restano invariati.

### 2026-10-27 — Checkpoint v22: dettaglio Edifici compatto e stati distinti di Costruisci

- L'utente conferma **«test passati» per v21**: il codice incluso corrisponde allo stress M4.8 realizzato in v18 (163 casi xUnit attesi, nove render e tre benchmark). La conferma è registrata come esito dell'utente; non sono disponibili TRX, PNG o i JSON con i valori numerici da analizzare.
- Nella GUI provvisoria, `Costruisci +1` distingue ora lo stato **normale verde scuro**, **hover verde più chiaro con bordo**, **pressione verde intenso** e **disabilitato grigio**. Il motivo di blocco e i prerequisiti della struttura selezionata sono spostati nella parte **fissa sopra i comandi**, così non richiedono lo scorrimento del dettaglio.
- Ridotti margini e spaziature di sidebar, testata, schede riassuntive, elenco edifici, pannello dettagli, tabella Risorse e barra inferiore per usare meglio le finestre da 1040×740 e 1240×850. La destinazione grafica resta **pixel art retro 8-bit essenziale**; nessun intervento alla logica di produzione, ai prezzi, alle regole `civil-growth-4` o al salvataggio.
- Aggiunti **tre casi Desktop**: due geometrie per verificare che la diagnosi sia interamente visibile senza scroll e un controllo di colori hover/disabilitato. **166 casi xUnit attesi, non eseguiti** qui perché manca .NET; necessaria validazione Windows e ispezione del render compatto. Il gate M4.8 precedente rimane confermato dall'utente, mentre quello **GUI v22 è in attesa**.

### 2026-10-27 — Checkpoint v21 solo documentale: studio M5.2 su Forgia e Zecca

- Creato il [dossier M5.2](../../design/M5_2_FORGE_MINT_PREFEASIBILITY.md) su Forgia (Ferro+Carbone→Spade) e Zecca (Oro+Carbone→Monete), con ipotesi numerica A e alternativa B a costi maggiorati. Le fonti mostrano le relazioni, non valori numerici; **nessun costo, ricetta o prerequisito è approvato**.
- Quantificata la contesa sul Carbone per le proposte M5.1+M5.2: quattro trasformatori chiederebbero 4 Carbone/s contro 2/s di una sola miniera; considerati lavoro per fasi, accumulo preventivo di metalli e seconda miniera, con somme dei materiali e del personale ricontrollabili. Non sono risultati di simulazione .NET.
- Esplicitata la decisione **D0.8**: Spade consumabili o equipaggiamento persistente; Monete come scorta usata da specifici comandi o valuta generale; ruolo futuro delle fonti coloniali. Preparati sette profili di test futuri, senza aggiungere codice al gioco.
- Allineati dossier M5.1, roadmap, gate D0, game design, handoff e README. **Versione eseguibile invariata da v18, ancora da validare in M4.8; ultimo gate riferito superato dall'utente v17, 163 test xUnit attesi.**

### 2026-10-27 — Checkpoint v20 solo documentale: prefattibilità industriale M5.1

- Preparato lo [studio industriale](../../design/M5_1_INDUSTRY_PREFEASIBILITY.md) su **M5.1a ferro/carbone**, **M5.1b oro**, **M5.1c misura di scala e contesa**. Il riferimento originale documenta le relazioni fra le filiere, non valori numerici.
- Proposto un set A *non approvato* di cinque edifici con costi, ricette, lavoratori e prerequisiti: primo ferro teorico a **450 legno lavorato / 320 pietra**, tre addetti, **1 ferro/s**; set B con costi 1,5× per confrontare la progressione. Documentati saturazione del carbone, deposito, output differito e bilanciamento con Cibo/Pane.
- Esplicitate due alternative di migrazione da M4 a M5 (**estendere lo scenario civile** o **creare una campagna con importazione dichiarata**), più test richiesti per Core, proiezioni, SQLite, UI e benchmark. **Nessuna decisione è approvata o implementata**: il codice e il validatore sono identici alla v18, il gate M4.8 resta in attesa.
- Aggiunta allo [stress M4.8](../../scenarios/BREAD_HIGH_DENSITY_BENCHMARK.md) una dimostrazione di capacità senza .NET: in 1.800 s la domanda minima è **8.424 Pane**, la snella può fornirne al massimo **8.200** inclusa la scorta iniziale; manca quindi almeno **224 Pane** per un rifornimento ininterrotto. Non si deducono da questo numero i secondi di blocco o la popolazione finale effettiva.
- Allineati roadmap, documenti D0, handoff, game design, salvataggi e README al checkpoint documentale. La GUI finale resta pixel art retro 8-bit essenziale, senza posizionamento sulla mappa.

### 2026-10-27 — Checkpoint v19 solo documentale: preparazione del gate D0 prima di M5

- Preparato il [dossier decisionale pre-M5](../../design/PRE_M5_DECISION_GATE.md) con inventario **di tutti i 43 tipi strutturali dei riferimenti**, mappatura dei **12 equivalenti presenti** nel gioco e classificazione dei **31** da valutare. Non sono stati aggiunti edifici al catalogo.
- Confrontate alternative per vittoria, ruolo delle isole e rapporto fra popolazione civile/militare; proposti pacchetti D0.1–D0.7, criteri di raggiungibilità e vincolo massimo aritmetico di **488 abitanti dopo otto ore** senza blocchi. Ogni preferenza resta una **proposta**, non un requisito approvato.
- Allineati roadmap, game design, README, direzione GUI 8-bit, studio Carne e handoff al gate **v17 riferito superato**, con nuovo stress **v18 ancora da validare**. Nessun file del motore, codice, test, script di validazione, SQLite, GUI o salvataggi modificato rispetto a v18.

### 2026-10-27 — M4.8: stress Pane ad alta densità e conferma del gate v17

- L'utente ha riferito superati i test e la validazione del checkpoint **v17**, comprensivi del confronto Pane `lean-chain` e della diagnostica M4.6/M4.7 (**163 casi xUnit attesi**). Non sono disponibili i report TRX/JSON grezzi: il superamento è registrato come conferma dell'utente, non come prova rieseguita localmente.
- Aggiunto `--benchmark-bread-high-density`: confronto autonomo delle quattro strategie da **320 abitanti**, 360 posti, quattro filiere Cibo, 100 Pane di riserva e 1.800 secondi, con tempi per 340/350 abitanti e rendicontazione delle scorte. Il benchmark M4.7 da 220 abitanti rimane disponibile e invariato nelle ipotesi.
- Estesa la validazione generale con il terzo JSON `bread-high-density-benchmark.json`, la riconciliazione delle 4 × 1.800 transizioni, i prezzi e il personale, e le ipotesi da verificare sull'ordine di crescita delle filiere. Documentati scenari, percorso dei report, roadmap e handoff.
- **Nuovo gate M4.8 implementato ma non compilato o eseguito con .NET in questo ambiente.** Restano 163 casi xUnit attesi. Nessuna modifica alle regole `civil-growth-4`, al motore, ai cataloghi, alla GUI, a SQLite o ai salvataggi.

### 2026-10-27 — M4.7: quarto benchmark Pane con filiera snella

- Esteso `--benchmark-bread-scale` con `lean-chain`: potenziamento immediato di **1 Fattoria + 1 Pozzo + 1 Mulino + 1 Forno**, acquistati e dotati di personale con i comandi del Core; +4 lavoratori, **360 legno lavorato e 250 pietra**, contro +5/420/280 del potenziamento completo.
- I quattro scenari partono dalla stessa colonia sintetica da 220 abitanti e affrontano 1.800 passi ciascuno; il report misura ora secondi-lavoratore per Fattorie/Pozzi/Mulini/Forni, Pane realmente prodotto e consumato e scorte intermedie finali. Contabilità del Pane e previsione economica vengono riconciliate con lo stato effettivo.
- Esteso `scripts/validate.ps1` a **quattro** politiche con controlli su prezzi, addetti e relazione tra crescita delle strategie; aggiornati `--help`, [studio del benchmark](../../scenarios/BREAD_SCALE_BENCHMARK.md), [modello teorico](../../scenarios/BREAD_CHAIN_CAPACITY.md), [roadmap](../../ROADMAP.md), [handoff](../../HANDOFF.md) e [gate M4](../../validation/M4_CIVIL_GROWTH.md).
- **Implementato, non ancora compilato o eseguito con .NET in questo ambiente.** Restano 163 casi xUnit attesi; il nuovo benchmark non aggiunge un test xUnit ma estende il validatore. Nessuna modifica a `civil-growth-4`, catalogo, SQLite, GUI o salvataggi. La capacità teorica di **4,5 Pane/s** non è dichiarata produttività realmente osservata.

### 2026-10-27 — Documentazione: roadmap operativa e formato dei resoconti

- Riorganizzata la [roadmap](../../ROADMAP.md) per distinguere M0–M3 completate, M4.1–M4.5 confermate, diagnostica M4.6 da validare, benchmark della filiera snella M4.7 da sviluppare e criteri di chiusura M4.8.
- Pianificato il gate di design **prima di M5** per catalogo, condizione di vittoria, conquiste, popolazione militare e tempo; dettagliate M5 industria/esercito/isole, M6 vittoria/bilanciamento e M7 qualità/rilascio. Confermata l'introduzione graduale della **GUI pixel art retro 8-bit essenziale** dopo la stabilizzazione M4.
- Allineati [game design](../../GAME_DESIGN.md), [handoff](../../HANDOFF.md), [convenzioni di sviluppo](../../DEVELOPMENT.md), [direzione grafica](../../PIXEL_ART_UI.md) e README. Ogni resoconto futuro indicherà stato, fatto, in corso, prossimi passi e verifiche aperte.
- **Solo documentazione**: nessuna modifica a sorgenti, regole `civil-growth-4`, catalogo, SQLite, asset o test. Il gate v14–v15 resta **163 casi xUnit attesi, non ancora verificati**.

### 2026-10-27 — M4: limite produttivo dell'intera filiera Pane

- Aggiunto un proiettore Application che segue a ritroso le ricette univoche di Pane, Farina, Acqua e Grano: per ogni anello espone fabbisogno medio, produzione nominale, deficit, addetti presenti e minimi teorici. Non presenta una stima in cataloghi con produttori alternativi, coprodotti o cicli, evitando suggerimenti non fondati.
- `GameView.BreadSupply.ChainCapacity` contiene l'**equivalente medio teorico in Pane/s** della filiera e gli anelli limitanti; resta distinto dalla produzione effettiva prevista nel prossimo secondo.
- Rafforzato il caso Application da 220 abitanti: una filiera completa (3 Pane/s), due Forni senza seconda filiera (ancora 3), Grano limitante (1,5), due filiere complete (6 nominali, ma zero Pane effettivamente previsto se mancano input). I test precedenti non sono stati modificati nel loro intento.
- Documentata la proposta di **filiera snella** (+4 lavoratori, 360 legno lavorato e 250 pietra, contro +5, 420 e 280 per il raddoppio): il vantaggio è soltanto un limite teorico, da confrontare poi con simulazioni reali. Nessun costo o meccanismo modificato.
- **163 casi xUnit attesi**, stesso numero del pacchetto precedente ma con nuove asserzioni; gate Windows ancora da eseguire. Versione `civil-growth-4`, SQLite e GUI attuale invariati; futura veste retro 8-bit predisposta a visualizzare gli anelli separatamente.

### 2026-10-27 — M4: deficit quantitativo Pane e capacità dei Forni

- L'utente ha confermato **test e validazione superati** anche per il benchmark di scala Pane a 220 abitanti del pacchetto precedente; non sono disponibili i relativi report grezzi.
- `GameView.BreadSupply` distingue ora domanda Pane, produzione nominale degli addetti, produzione prevista nel prossimo secondo, differenza nominale e numero minimo/addizionale di addetti quando esiste una sola ricetta per il Pane. Dati pronti per indicatori compatti nella futura GUI retro 8-bit.
- La sidebar indica, quando i Forni sono sottodimensionati, il deficit in Pane/s e il numero addizionale di addetti. La carenza di ingredienti continua a essere distinta dalla capacità nominale; nessun suggerimento acquista edifici in automatico.
- Aggiunto un test Application a 220 abitanti (212 avanzati): un Forno da 3 Pane/s contro domanda 3,180; due Forni senza ingredienti restano fermi. Rafforzati i test esistenti e il controllo di ingombro della sidebar a 1040×740. **163 casi xUnit attesi, nuovo gate da eseguire**; regole, formato SQLite e sprite invariati.

### 2026-10-27 — M4: nuova prova di scala Pane dopo gate utente superato

- Registrata la conferma dell'utente sul superamento di test e validazione della precedente versione (162 casi attesi); i report grezzi non sono stati allegati a questa sessione.
- Aggiunto `--benchmark-bread-scale`: partendo da uno snapshot sintetico a 220 abitanti, confronta una filiera Pane completa, due filiere immediate e la seconda costruita dopo 15 minuti. Le nuove costruzioni passano dai comandi reali, con costi e personale effettivi.
- Controlla secondo per secondo ricette, razioni, progresso e scorte; produce un riepilogo di popolazione, carenze, produttività e spesa aggiuntiva. Il validatore salva un JSON dedicato ed esegue controlli di coerenza e confronto fra le politiche.
- Nessuna modifica a `civil-growth-4`, ai salvataggi o alla GUI 8-bit pianificata. **Il nuovo benchmark non è ancora stato eseguito** in un ambiente .NET; le prove pregresse rimangono confermate dall'utente.

### 2026-10-27 — M4: proiezione della sostenibilità Pane riutilizzabile dalla GUI 8-bit

- Spostata la diagnosi del prossimo secondo da `MainViewModel` a un proiettore Application che espone `GameView.BreadSupply`, stato tipizzato e autonomia teorica della scorta. L'interfaccia Avalonia usa il modello senza calcolare produttività o confrontare input.
- Migliorata l'informazione prima del primo abitante avanzato: l'avviso sulla necessità del Pane resta visibile anche quando la crescita segnala prioritariamente **mancanza di Cibo**. Consumi, arrivi e progresso rimangono invariati.
- Aggiunti due test Application sui blocchi simultanei e sulle differenze fra carenza input, Forno operativo e deposito pieno; tre test Desktop controllano ora anche gli stati tipizzati.
- Nessuna variazione alle regole `civil-growth-4`, ai salvataggi o agli asset grafici. **162 casi xUnit attesi, non eseguiti** senza SDK .NET; ultimo checkpoint confermato: 140 test superati.

### 2026-10-27 — M4: progettazione della demolizione e direzione grafica 8-bit

- Registrata la scelta esplicita dell'utente: futura GUI **pixel art retro 8-bit essenziale**, mantenendo edifici in elenchi e schede. Aggiunti [direzione grafica](../../PIXEL_ART_UI.md) e [ADR 0012](../../adr/0012-retro-8bit-desktop-visual-language.md), senza modificare Desktop.
- Preparata la [valutazione della demolizione Q-08](../../scenarios/DEMOLITION_M4_REVIEW.md): rimborsi candidati 0/25/50%, scenari quantitativi e invarianti per abitanti, lavoratori, dipendenze e magazzini. Il 25% è solo proposto: nessuna demolizione abilitata.
- `civil-growth-4`, costo delle Case grandi, ricetta Pane, salvataggi e suite xUnit invariati. **Gate corrente: 160 casi attesi, non eseguiti**; ultimo checkpoint confermato: 140 test superati.

### 2026-10-27 — M4: ripresa da carenze di Pane e Cibo

- Corretto il caso iniziale della diagnosi UI: se una Casa grande offre posti ma manca ancora Pane per il **primo** residente avanzato, il messaggio operativo viene mostrato anche con zero residenti avanzati. Prima la diagnosi rimaneva nascosta proprio durante lo sblocco.
- Aggiunto un test Desktop per la sequenza 16→17 abitanti con 59 secondi di crescita già accumulati e Pane inizialmente a zero: Pane prodotto al primo secondo, arrivo al secondo, consumo del nuovo residente soltanto dal terzo.
- Aggiunto un test Application con Cibo iniziale a zero, Pesce già disponibile, un Deposito alimentare attivo e Pane in scorta: la carenza non consuma Pane, il Cibo prodotto diventa utilizzabile dal passo successivo e il progresso parziale viene conservato.
- Nessuna modifica alle regole, ai costi, alle ricette o al formato del salvataggio. **160 test xUnit attesi** (2 nuovi casi), **non eseguiti** con .NET. Ultimo checkpoint confermato: 140.

### 2026-10-27 — M4: diagnosi della sostenibilità del Pane nell'interfaccia

- Nella sezione Crescita, quando sono presenti residenti avanzati, la UI distingue capacità nominale insufficiente dei Forni, input momentaneamente insufficienti, deposito quasi pieno e produzione disponibile per il prossimo secondo.
- Mostrata l'autonomia della scorta di Pane **senza nuove forniture, al consumo corrente**; non è una previsione dell'esaurimento reale con produzione attiva né una garanzia di mantenimento futuro.
- Rafforzato il test Desktop della razione avanzata e aggiunto un nuovo caso che verifica un Forno fermo per Pane pieno: il saldo momentaneo negativo non va presentato come capacità nominale insufficiente. La vista compatta richiede ancora ispezione Windows.
- Predisposto un nono render, `civil-advanced-bread-compact.png`, a 1040×740, con controllo del posizionamento della diagnosi Pane. Il validatore verifica che tutti e nove i PNG siano stati generati nella **cartella della specifica esecuzione**, escludendo falsi esiti dovuti a immagini vecchie.
- Nessun cambiamento alle regole, alle ricette, alle altre risorse o al formato del salvataggio. **158 casi xUnit attesi**, ancora non eseguiti con .NET; ultimo checkpoint confermato: 140.

### 2026-10-27 — M4: riconciliazione dei bilanci e soglia produttiva del Pane

- Il confronto `--benchmark-housing` verifica ora a ogni secondo, per le tre strategie, l'equivalenza fra consumi e ricette previsti e scorte effettive, nonché arrivi e progresso della crescita. Registra `ReconciledSeconds`; il validatore richiede 28.800 secondi riconciliati per strategia.
- Aggiunto un test Core oltre 200 residenti avanzati: 1 Forno, 2 Forni dall'inizio e secondo Forno dopo 15 minuti. Il rinforzo produttivo deve migliorare la progressione, senza recuperare retroattivamente il tempo perso.
- Un modello aritmetico indipendente da .NET prevede 246, 250 e 249 abitanti dopo 30 minuti a partire da 220, nei tre casi rispettivi. Questi numeri **non sono stati verificati con .NET**. **157 test xUnit attesi**; ultimo checkpoint realmente verificato: 140. Regole, costi e salvataggi invariati.

### 2026-10-27 — M4: riproducibilità della progressione avanzata (gate .NET in attesa)

- Aggiunti due test Application di riproduzione fino a due ore: cronologia con chiamate da un secondo contro intervalli disomogenei, seconda Casa grande, confronto esatto di edifici, scorte, tempo, abitanti e progresso della crescita. Un salvataggio intermedio a 879,5 secondi verifica la ripresa dopo pausa e l'arrivo con gli ultimi 0,5 secondi.
- Aggiunto un test Infrastructure con SQLite reale per una colonia già fornita di Forno e Casa grande: salvataggio a 879,5 secondi, ricaricamento in pausa, ripresa e confronto con la partita mai interrotta.
- Nessuna modifica a formule, costi, versioni delle regole o formato di salvataggio. Conteggio xUnit **atteso: 156 casi** (inclusi i 3 nuovi), **non eseguiti** per assenza di SDK .NET. Checkpoint confermato: 140 test.

### 2026-10-27 — M4: confronto quantitativo di tre strategie (esecuzione .NET in attesa)

- Valutata la filiera della Carne sulla base dei riferimenti originali: aggiungerla come semplice duplicato del Pesce non risolve un vincolo del gioco; [documentate alternative e rinvio possibile a M5](../../scenarios/MEAT_DESIGN_REVIEW.md), senza cambiare il catalogo.
- Eseguito un controllo aritmetico indipendente, esterno a .NET, sulle tre cronologie: le soglie e i materiali attesi coincidono. Il Magazzino aumenta la scorta di Pane senza anticipare gli arrivi nelle politiche considerate; il risultato rimane **preliminare** e non sostituisce l'esecuzione del motore.
- Rafforzati i controlli del benchmark sulla diagnostica di attività/capacità del Forno, un test Core sul mancato consumo di Pane quando manca Cibo e la prova di conversione SQLite di un checkpoint storico con Forno e lavoratori già presenti. Il totale xUnit atteso sale a **153**, ancora non eseguiti.
- Preparato `--benchmark-housing` nel simulatore: confronta Case piccole, Case grandi standard e Case grandi anticipate con secondo Pescatore e Magazzino, su otto ore simulate. Registra tempi per 48/100/136 abitanti, spese, inattività del Forno, carenze e posti occupati.
- La validazione ora prevede anche un confronto JSON e controlli sulle soglie teoriche e i materiali; i tre profili sono descritti in [HOUSING_BENCHMARK](../../scenarios/HOUSING_BENCHMARK.md). **Non eseguito:** ultimo checkpoint confermato ancora 140 test, nessuna modifica alle regole `civil-growth-4` o ai salvataggi.
- Il confronto con la Casa piccola aggiuntiva necessaria alla filiera nei profili di riferimento mostra un pareggio più prudente: a quattro Case grandi, risparmio previsto di 140 legno lavorato e parità di pietra, con otto posti aggiuntivi. Non è una conclusione sull'ottimalità.

### 2026-10-27 — M4: Casa grande, razioni Pane e regole `civil-growth-4` (gate Windows in attesa)

- Approvato il modello quantitativo: Casa grande (+32 posti, costo 160 legno lavorato + 80 pietra, Forno prerequisito); il Forno produce 3 Pane/s, risorsa distinta da Cibo, con capacità iniziale 100 (+500 per Magazzino).
- Consumano tutti 0,010 Cibo/abitante/s; chi supera i posti delle Case piccole richiede inoltre 0,015 Pane/abitante/s. Scorte insufficienti sospendono la crescita, senza rimuovere abitanti o resettare i progressi. Core, viste e UI mostrano residenti avanzati, fabbisogno e carenze.
- Aggiunta conversione dichiarata `civil-growth-3 → civil-growth-4`, concatenabile da `-1/-2`: nuove Case grandi a zero, nuovo Pane a zero, nessuna trasformazione del Cibo pregresso. Il formato SQLite/snapshot resta v2 e il primo salvataggio conserva la copia dell'originale.
- Aggiunti test per razioni, soglia 16→17 residenti, recupero dopo carenze, capacità del Pane, interfaccia e SQLite; nuove cronologie `bread-start`, `advanced` a 880 secondi e `advanced` a 8 ore nello script di validazione. Aggiornati [scenario](../../scenarios/CIVIL_GROWTH.md), [ADR 0011](../../adr/0011-advanced-housing-and-bread-rations.md), formato e handoff.
- **Non ancora verificati con build, test o render Windows:** l'ambiente corrente non dispone di .NET. Ultimo checkpoint confermato: **140 test superati**.

### 2026-10-27 — Studio M4: progressione abitativa e pane (proposta poi approvata)

- Preparato `docs/scenarios/ADVANCED_HOUSING_PROPOSAL.md`: confronto fra prerequisito semplice e risorsa Pane distinta, ipotesi Casa grande da 32 posti a costo 160/80, consumo aggiuntivo 0,015 Pane/abitante avanzato/s, capacità Pane 100, punto di pareggio dei materiali dalla quarta Casa grande.
- Documentati impatto su Core, UI e conversioni, rischi di scorta/produzione e test necessari. Nello studio iniziale non erano state modificate regole o salvataggi. L'utente ha poi approvato il modello, implementato nell'incremento `civil-growth-4` sopra descritto; i benchmark effettivi restano da eseguire.

### 2026-10-27 — M4: diagnostica dei colli di bottiglia (validazione Windows in attesa)

- Il pianificatore espone, per ogni struttura, gli identificatori degli input insufficienti e delle risorse di uscita senza spazio, usando le stesse prenotazioni, priorità e regole del passo economico. La proiezione applicativa ne presenta i nomi leggibili; nel ciclo di avanzamento al secondo la raccolta diagnostica è disabilitata per evitare allocazioni superflue.
- Nel dettaglio Edifici compaiono le cause specifiche (per esempio pesce mancante o cibo senza capacità), i prerequisiti edilizi assenti e la richiesta di personale. Le strutture parzialmente attive indicano il numero di lavoratori produttivi.
- Aggiunti tre test per diagnostica Core, proiezione Application e dettaglio Avalonia; rafforzati test esistenti sulla contesa di input e capacità.
- Nessun nuovo pacchetto, regola di gioco, conversione o modifica al formato SQLite. **I test nuovi e i render sono ancora da eseguire su Windows**: nell'ambiente di preparazione non è disponibile `dotnet`. Restano confermati solo i 140 test del checkpoint precedente.

### 2026-10-27 — Script di pulizia

- Aggiunti `clean.cmd` e `scripts/clean.ps1`: cancellano `bin` e `obj` di ogni progetto del repository, con `-DryRun` per l'anteprima. Esclusi `.git`, `.local-cache` e collegamenti.

### 2026-10-27 — M4: analisi di raggiungibilità del catalogo

- Aggiunto `CatalogReachability.Analyze` nel Core: controllo statico che ogni struttura sia costruibile (prerequisiti, costi pagabili con risorse rinnovabili o con la dotazione iniziale, capacità di base o ampliabile) e che ogni ricetta abbia input rinnovabili ed entri nella capacità. Ignora tempo, persone e contesa.
- Simulatore: `--check-catalog [--scenario …]`, con codice di uscita 1 se uno sblocco è impossibile. La validazione lo esegue per entrambi gli scenari e salva i report.
- Test sugli scenari distribuiti (completi) e sulla catena delle conversioni di `civil-growth`. **140 test superati**; regole e salvataggi invariati.

### 2026-10-27 — M4: filiera del pane e regole `civil-growth-3`

- Aggiunti grano, acqua e farina e quattro strutture: fattoria (1 grano/s), pozzo (1 acqua/s), mulino (2 grano → 1 farina) e forno (1 farina + 1 acqua → 3 cibo), con prerequisiti mulino→fattoria e forno→mulino+pozzo. Valori provvisori: il pane affianca il pesce come fonte di cibo, più efficiente per lavoratore ma più costosa.
- Nuove regole `civil-growth-3`. Le conversioni sono dichiarate in `CivilGrowthScenario.Changes` e si concatenano: i checkpoint `civil-growth-1` e `civil-growth-2` vengono caricati con le nuove strutture a zero e l'originale viene copiato come prima.
- La tabella della vista Risorse ora scorre, con intestazione e nota fisse: le otto risorse non stavano nella finestra compatta.
- Simulatore: cronologia `bread`, inclusa nella validazione. Verificati **132 test**, nove simulazioni, otto render e ciclo nativo Windows. Aggiornati scenario, formato, report M4, roadmap e handoff.

### 2026-10-27 — M4: magazzino espandibile e regole `civil-growth-2`

- Aggiunto il Magazzino (100 legno lavorato + 60 pietra, senza personale): +500 capacità per ognuna delle cinque risorse. Valori provvisori scelti dall'utente.
- Introdotto l'effetto `StoragePerBuilding` nelle definizioni: la capacità effettiva deriva dagli edifici e viene usata da costruzione, previsione, avanzamento, ripristino e vista Risorse. Limite numerico atomico.
- Nuove regole `civil-growth-2`. I checkpoint `civil-growth-1` vengono convertiti al caricamento con zero magazzini e il desktop lo spiega; regole sconosciute restano rifiutate.
- L'adattatore SQLite copia il checkpoint in `<nome>.<regole>.backup.sqlite3` prima di sostituirlo con scenario o regole diversi, senza mai sovrascrivere copie esistenti. Formato file/snapshot invariato (v2).
- UI: descrizione, effetto e stato del magazzino. Simulatore: cronologia `storage`, inclusa nella validazione.
- Verificati **128 test**, build senza avvisi, otto simulazioni, otto render e ciclo nativo Windows. Aggiunto l'[ADR 0010](../../adr/0010-storage-capacity-and-rules-conversion.md); aggiornati scenario, formato, report M4, roadmap e handoff.

### 2026-10-26 — M4: case, crescita e prima filiera alimentare

- Aggiunto lo scenario giocabile `civil-growth-1`, con casa piccola, pescatore e deposito che trasforma pesce in cibo, oltre ai tre produttori di base.
- Introdotti capacità abitativa, consumo civile esatto e crescita di persone intere con accumulo dei secondi validi; fame, pausa e case piene sospendono gli arrivi senza perdere il progresso.
- Condivise prenotazioni alimentari e ricette tra forecast e avanzamento. La previsione delle risorse include il consumo degli abitanti.
- Aggiunti posti abitativi, stato della crescita e fabbisogno nell'interfaccia; distinte strutture abitative e produttive e saldi negativi delle risorse.
- Creato snapshot/file SQLite v2 per la crescita parziale. Il nuovo default usa `civil-growth.sqlite3`; Foundation v1 resta disponibile con `--scenario economy-foundation` e non viene convertito o sovrascritto automaticamente.
- "Nuova partita" mantiene lo scenario corrente. Aggiunti argomenti del simulatore e cronologie `bootstrap` e `recovery`.
- Verificati **117 test**, build senza avvisi, sette simulazioni fino a 32 ore, otto render e ciclo nativo Windows per entrambi gli scenari, inclusa la ripresa a 59 secondi con un solo arrivo successivo.
- Aggiornati scenario, ADR 0009, formato dei salvataggi, roadmap e handoff. Magazzini espandibili e ulteriori filiere civili restano da implementare in M4; valori e durata del gioco completo non sono ancora bilanciati.

### 2026-10-26 — Avalonia 12 e preparazione dello scenario M4

- Aggiornata la soluzione definitiva ad Avalonia **12.1.4**, con lock file coerenti; la prova storica M1 resta separata.
- Migrati i test Desktop a xUnit 3.2.2, configurato HarfBuzz per il rendering headless e aggiornata l'esportazione PNG. Timer associati esplicitamente al dispatcher della finestra.
- Aggiornato Microsoft.Data.Sqlite a **10.0.14**, che risolve SQLitePCLRaw 2.1.13 e SQLite nativo 3.53.3, dopo aver rilevato l'advisory della precedente libreria nativa.
- Aggiunta una fixture di checkpoint M3 e verificato il ripristino esatto con il nuovo provider, senza modifiche al formato v1 o alle regole economiche.
- Build Release senza errori o avvisi; **83 test superati**, quattro render, simulazioni e ciclo nativo Windows completati. Il riepilogo di validazione registra anche le versioni dei pacchetti risolte.
- Confrontate 63 coppie pacchetto/versione con il feed delle vulnerabilità NuGet: nessuna corrispondenza dopo l'aggiornamento. L'audit CLI resta impedito dall'errore TLS dell'ambiente; dettagli nel [report](../../validation/AVALONIA_12.md).
- Registrato l'[ADR 0008](../../adr/0008-avalonia-12.md) e preparata una [scheda M4](../../scenarios/CIVIL_GROWTH_DRAFT.md) con valori candidati per abitazioni, crescita e alimentazione, ordine del passo e prove richieste. Queste nuove regole non sono ancora implementate.

### 2026-10-26 — M3: prima partita desktop salvabile

#### Aggiunto

- Desktop Avalonia/MVVM e Infrastructure collegati alla soluzione definitiva, con quattro progetti di test complessivi e nessun riferimento ai prototipi.
- Viste Edifici e Risorse, costi e ricette, assegnazioni e costruzioni da 1/5/10 unità, produzione/consumo/saldo previsti al secondo e capacità visibili.
- Port applicativo di persistenza, salvataggi fuori dal dispatcher e controllo di revisione contro caricamenti tardivi.
- Formato SQLite v1 con metadati, quantità in millesimi, edifici e assegnazioni in una transazione atomica.
- Caricamento iniziale in pausa, checkpoint ogni minuto, salvataggio manuale e alla chiusura; conservazione della sessione se la scrittura fallisce.
- Protezione di file estranei e versioni future, conferma di nuova partita e uscita senza salvataggio solo su scelta esplicita dopo errore.
- Orologio monotono, pausa alla minimizzazione e gestione delle notifiche di sospensione/ripresa Windows, con fallback per interruzioni lunghe.
- Comando `run-game.cmd`, pubblicazione Windows nel validatore, documentazione del formato e ADR 0007.

#### Verificato

- Build Release senza errori o avvisi; **82 test superati** (36 Core, 18 Application, 12 Infrastructure, 16 Desktop).
- Quattro render a 1240×850 e 1040×740; simulazioni di 60 secondi, 8 e 32 ore.
- Pacchetto nativo Windows: salvataggio, nuova sessione, ricaricamento, sospensione simulata e chiusura completati con codice zero.
- Comando di avvio interattivo verificato in modalità smoke, senza usare il salvataggio personale.

Baseline locale Avalonia 11.3.18 e Microsoft.Data.Sqlite 10.0.9. L'audit online è stato ritentato e non completato per errore TLS. Popolazione dinamica, alimentazione, catalogo completo, militari, isole e bilanciamento restano nelle milestone successive.

### 2026-10-26 — Nuovi riferimenti di risorse e filiere

- Analizzate due schermate delle risorse e tre diagrammi di materiali, alimentazione, monete, esercito, colonie e strutture decorative.
- Aggiornato l'inventario a nove immagini e documentati i collegamenti visibili, le ambiguità e le conseguenze sulle milestone successive.
- Distinte quantità, capacità e variazioni senza attribuire unità temporali non mostrate; mantenuti separati abitanti, materiali, missioni e mezzi.
- Aggiornati game design e handoff. Nessuna modifica al codice, ai parametri dello scenario o alle immagini.

### 2026-10-26 — M2: motore economico e casi d'uso

#### Aggiunto

- Soluzione definitiva `SimRisorse.slnx` con Core, Application, due progetti di test e strumento Simulation, senza riferimenti a bozza o esperimento.
- Catalogo immutabile con identificatori stabili, validazione di riferimenti e cicli dei prerequisiti.
- Risorse esatte in millesimi su `long`, capacità, acquisti multipli atomici e assegnazioni intere dei lavoratori.
- Pianificatore unico per produzione e previsioni, con prenotazioni di materiali e spazio per priorità esplicita.
- Tempo di gioco esplicito, pausa, residui inferiori al secondo e snapshot immutabili validati.
- Funzionalità verticali per nuova partita, costruzione, assegnazione, rilascio, pausa, avanzamento e lettura dello stato.
- Coordinamento delle mutazioni e delle proiezioni per impedire doppie spese o assegnazioni concorrenti.
- Scenario economico di riferimento, ADR 0006 e report M2.
- Configurazione SDK, build, stile e pacchetti; lock file e comando `validate.cmd`.

#### Verificato

- Build Release con zero errori e avvisi; 45 test superati (36 Core, 9 Application e architettura).
- Simulazioni senza finestra di 60 secondi, 8 e 32 ore, con tempo e quantità entro i limiti attesi.
- Configurazioni storiche della bozza e pacchetti dell'esperimento rimasti separati.

L'audit online delle dipendenze è stato tentato, ma non completato a causa di un errore TLS dell'ambiente. La nuova UI, l'adattatore SQLite definitivo e il bilanciamento completo restano lavoro successivo.

### 2026-10-26 — M1: scenario ridotto e prova desktop

#### Aggiunto

- Scheda del primo scenario economico: tre strutture, otto abitanti, acquisti e assegnazioni manuali, ricette e pausa.
- Prova eseguibile isolata in `experiments/stack-probe/`, con modello indipendente dalla UI, finestra Avalonia/MVVM e adattatore SQLite.
- Salvataggio transazionale, caricamento validato e ripresa in pausa, compreso il tempo residuo inferiore al secondo.
- 23 test per economia, tempo, conservazione dei lavoratori, persistenza e interfaccia headless con rendering Skia.
- Script riproducibile di ripristino, build, test, pubblicazione e avvio nativo di verifica; comando per aprire la prova interattiva.
- Versioni centralizzate e lock file limitati all'esperimento.
- Report M1 con evidenze e limiti delle verifiche.

#### Verificato e deciso

- Build Release con zero errori e avvisi; tutti i 23 test superati; pacchetto framework-dependent avviato su Windows.
- Rendering a 1180×790 e 1020×720, con comandi principali visibili; cronometro oltre 24 ore.
- Adozione di C#/.NET 10, Avalonia/MVVM e SQLite per la prima implementazione desktop, registrata nell'ADR 0005.

La prova usa pacchetti già disponibili offline: non certifica versioni più recenti, sicurezza delle dipendenze, altri sistemi operativi o bilanciamento del gioco completo. Il Core definitivo resta M2.

### 2026-10-26 — Impostazione del progetto

#### Aggiunto

- README con descrizione, stato reale del progetto e percorsi di lettura.
- Game design con scelte confermate, proposte e registro delle questioni aperte.
- Architettura prevista, convenzioni di sviluppo e criteri di verifica.
- Registro ADR con cinque decisioni/proposte e modello per quelle successive.
- Roadmap per milestone e handoff per la ripresa del lavoro.
- Indice e analisi dei quattro screenshot forniti come riferimento.
- README del prototipo con limiti noti e istruzioni per aprirlo.

#### Riorganizzato

- Soluzione, sorgenti e test Windows Forms trasferiti in `legacy/winforms/`, senza modifiche al loro contenuto e mantenendo i riferimenti relativi.
- Immagini trasferite da `screnshot/` a `docs/references/screenshots/`, senza rinominarle.
- Cache Visual Studio, output `bin/` e `obj/`, preferenze locali del progetto e cartelle vuote di analisi raccolti in `.local-cache/`. La pulizia è reversibile: questi file non sono stati cancellati.

#### Configurazione

- Aggiunte esclusioni Git mirate per cache locali, dati di gioco locali e file ausiliari SQLite.

### Limiti della modifica

- Nessuna modifica alle meccaniche della bozza.
- Nessuna migrazione del runtime, nuova applicazione Avalonia o persistenza SQLite implementata.
- Nessuna promessa di completamento in otto ore verificata attraverso simulazioni.

- E3b/4c RC3: la chiusura della finestra non forza il salvataggio se il database v14 non è scrivibile o non è riconosciuto. Test di regressione dedicato.
