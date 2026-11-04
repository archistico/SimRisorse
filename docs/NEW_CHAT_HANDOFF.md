# Nuova chat — RC40 / F1-D01 (2026-11-04)

**Base da usare: RC40 integrale.** Ultima suite confermata su Windows: **RC38 541/541**. RC39 545 test non confermati; RC40 552 attesi, non compilati in questo ambiente. L'utente ha esplicitamente approvato la nuova architettura alimentare: Pesce, Pane, Carne (Stalla e Caccia come due ingressi al Macellaio), Frutta e Verdura; solo il Deposito alimentare, con lavoratori e capienza, genera Cibo ordinario. Quota di Pane intoccabile per residenti avanzati; il resto può diventare Cibo; nessuna unità va contata due volte. Orto/Frutteto separati. Il Macellaio dovrà avere ricette alternative automatiche, nessuna combobox.

**RC40 ha solo il dominio isolato** `FoodDepotModel.cs` e sette test Core; NON è attivo in v14, NON ha nuove risorse visibili e NON cambia la versione del DB. Non aggiornare i salvataggi in questa fase. La nuova roadmap ufficiale è F1-D02 carne → D03 ortofrutta → D04 deposito/consumi → D05 migrazione/stabilità → E4a nuova economia → E4b coefficienti approvati → E4c → navale. RC39 ed E4a precedente rimangono storici/not-run; non inventare benchmark. Conservare GUI v14 predefinita e v13 legacy isolata. Consegnare ZIP integrale con ADR/changelog/roadmap/handoff.

[Modello](design/F1_D01_RC40_MODELLO_ALIMENTARE.md) · [Roadmap](ROADMAP.md) · [ADR](adr/0050-filiera-alimentare-unificata-deposito.md) · [Test plan](validation/F1_D01_RC40_TEST_PLAN.md)

---

# Nuova chat — RC39 / E4a-D02 (2026-11-04)

**Ripartire dall'archivio RC39 integrale.** **RC38 ha superato 541/541 test Windows** su conferma dell'utente. **RC39 candidata: 545 test attesi**, non eseguiti in questo ambiente. Nuovo `E4aNaturalBenchmark` per E4A-01…05, politica `natural-reference-v1`, avvio da partita reale appena creata (nessuna aggiunta di stock/popolo/ricerca), comandi v14, serie da 60 s, log di tentativi, `pass`/`timeout`/`blocked`, export CSV e JSON via `--benchmark-e4a-natural`. Quattro test nuovi e ADR 0049.

**Non sono stati ancora misurati tempi naturali:** `benchmarks/e4a/scenarios.csv` resta `not-run` per tutte le righe. E4A-06/07 restano runner separati con finanziamento controllato. Le cinque strategie manifest sono etichette-obiettivo, non algoritmi ottimizzati distinti. Non inventare risultati. Nessun cambiamento di GUI (v14 ordinaria; v13 `--legacy-v13`), Core, SQLite, salvataggi o coefficiente E4b.

**Prossimo gate:** `./validate.cmd` per 545 test; poi due run indipendenti E4A-01 e un run E4A-01…05, salvare gli esiti. Proseguire E4a-D03 Carbone/stoccaggio, D04 persistenza e analisi, E4b **solo su approvazione dei valori**. Consegna sempre archivio ZIP integrale con roadmap, changelog e handoff.

[Rapporto](design/E4A_D02_RC39_BENCHMARK_NATURALI.md) · [Piano](validation/E4A_D02_RC39_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [ADR 0049](adr/0049-benchmark-naturali-politica-esplicita.md)

---

# Nuova chat — RC38 / E4a-D01-F1 (2026-11-04)

Ripartire da RC38. RC37 ha completato l'esecuzione di **541 test** con **540 superati e un fallimento** in `V14SchoolResearchEndToEndTests`, dovuto all'ordine di `OtherBuildings` restituiti da SQLite. RC38 modifica soltanto il test: confronta per chiave edifici, famiglie, contatori e tecnologie; i progressi degli studenti restano posizionali. **RC38 non validata: 541 attesi.** Invariati gioco, GUI, salvataggi, benchmark. Ultima suite interamente confermata RC30/511. Procedere con `./validate.cmd` su Windows e poi benchmark controllato E4A-06/07; scenari ancora `not-run`. Conservare v14 predefinita, v13 `--legacy-v13`, slot separati, e nessun E4b senza misure e approvazione.

[Rapporto](design/E4A_D01_RC38_FIX_ORDERING_SQLITE.md) · [Piano Windows](validation/E4A_D01_RC38_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Nuova chat — SimRisorse RC37 / E4a-D01 (2026-11-04)

**Riprendere dal pacchetto RC37 integrale**, leggendo `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/design/E4A_D01_RC37_BENCHMARK_SCUOLE.md` e `docs/validation/E4A_D01_RC37_TEST_PLAN.md`. **Ultimo checkpoint Windows confermato: RC30 / 511 test. RC31–RC37 NON validate; RC37: 541 test cumulativi attesi.** L'utente non può testare ora: NON attribuire esiti verdi, NON generare numeri inventati.

RC37 aggiunge `SimRisorse.Application.Benchmarks.E4aSchoolBenchmark` e il comando del tool `--benchmark-e4a-school [--output DIRECTORY]` per E4A-06/07. Fixture dichiarata +600 Legno lavorato/+400 Pietra/+10 Carbone; costruzioni, Matite, iscrizione e avanzamento attraverso comandi reali, campionamento di 30 s, CSV/JSON non sovrascrivibili. I 10 scenari nel manifest restano `not-run` fino alla misurazione. Tre nuovi test; nessun cambio gameplay, UI v13/v14, prezzi, SQLite, salvataggi.

v14 è il gioco ordinario (`.\run-game.cmd`); v13 solo `--legacy-v13` e slot diverso. **P1-D50 non formalmente approvata senza gate; E4b rimandata.** Prossimo lavoro E4a-D02 (strategie naturali) dopo il checkpoint, oppure continuare con strumentazione D02 se l'utente non può testare. Consegna sempre ZIP integrale con roadmap/changelog/ADR/handoff.

[Rapporto](design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) · [Piano](validation/E4A_D01_RC37_TEST_PLAN.md) · [ADR 0048](adr/0048-benchmark-scuole-controllato.md).

---

# Nuova chat — SimRisorse RC36 / P1-D50

Usare lo ZIP **RC36 integrale** e leggere prima `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/design/P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md` e `docs/validation/P1_D50_RC36_TEST_PLAN.md`. **Ultimo gate Windows confermato:** RC30 (511 test). **RC31–RC36 NON ancora validate**; RC36 richiede **538 test cumulativi**, con due test SQLite supplementari su vecchi studenti e ledger di rimborso. Non attribuire loro esito verde finché l'utente non conferma `validate.cmd`.

La v14 è la GUI ordinaria, la v13 è accessibile solo con `--legacy-v13`; non eliminare la GUI precedente o unire automaticamente i salvataggi. Codice applicativo invariato in RC36, nessun E4b. **P1-D50 implementata ma non chiusa; E4a è pronta come protocollo/manifest, non ci sono ancora benchmark misurati.** Prossimo step: gate RC36 quando possibile, poi E4a con scenari naturali e controllati esplicitamente separati; solo in E4b sottoporre prezzi/ricette/tempi ad approvazione.

[Protocollo E4a](design/E4A_PROTOCOLLO_BENCHMARK.md) · [Scenari](../benchmarks/e4a/scenarios.csv) · [ADR 0047](adr/0047-gate-p1-e-misure-e4a.md)

---

# NEW CHAT HANDOFF — SimRisorse RC35 / P1-D49

**Baseline di lavoro:** RC35 completa, con tre test end-to-end su sessione v14 e `V14SqliteSaveStore`. **Conferma Windows più recente:** RC30, 511 test. **Candidati cumulativi RC31–RC35:** 536 test attesi, non eseguiti in questa sessione. Non affermare che siano passati. Nessun cambiamento ai sorgenti applicativi, ai salvataggi o alla GUI rispetto a RC34; nuova copertura `tests/SimRisorse.Infrastructure.Tests/V14SchoolResearchEndToEndTests.cs`.

**Ciclo provato dai nuovi test:** finanziamento della fixture entro la capacità Municipio, costruzioni Pescatore/Deposito alimentare/Scuola, Matite, iscrizione, qualifica, assegnazione, 99,900→100,000 punti con ripresa da SQLite, acquisto Lavorazioni I, upgrade Segheria Grande, secondo salvataggio e recupero in pausa. Altri test: formazione interrotta e fallimenti senza mutazione.

**Prossimo passo:** P1-D50: riepilogo P1, verifica dei 536 test su Windows quando possibile, consolidamento della documentazione e preparazione E4a *senza intervenire ancora sui costi*. GUI v14 predefinita; `--legacy-v13` solo per recupero, slot protetti. Valori 120s, 10 Cibo, 4 Matite/lotto, 0,1 punti/s restano candidati. Requisito di almeno 2 Matite/allievo e 1/3/10 lavoratori per tier confermati.

[Roadmap](ROADMAP.md) · [Rapporto P1-D49](design/P1_D49_RC35_INTEGRAZIONE_PROGRESSIVA.md) · [Piano test](validation/P1_D49_RC35_TEST_PLAN.md) · [ADR 0046](adr/0046-catena-integrata-scuola-ricerca-tier.md)

---

# Handoff prioritario — RC34 / P1-D48

**Archivio di riferimento: RC34 integrale.** RC30 ultima validazione Windows confermata (511 test); RC31–RC34 ancora da compilare/testare cumulativamente (533 attesi). V14 predefinita, v13 legacy separata. P1-D48 introduce controllo dei benefici reali delle otto tecnologie acquistabili e descrizioni dei tier sbloccati; 3 ricerche future restano non acquistabili. Nessun cambio a economia, SQLite o Scuola.

P1-D49 integrazione completa e salvataggi, P1-D50 gate P1, E4a misure, E4b valori approvati, E4c consolidamento, poi navale. **Non dichiarare PASS non eseguiti**. [Rapporto](design/P1_D48_RC34_ALBERO_TECNOLOGICO.md) · [Test](validation/P1_D48_RC34_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff prioritario — RC33 / P1-D47

Partire dallo **ZIP integrale RC33**. Ultimo checkpoint confermato RC30 (511 test). RC31–RC33 **da testare su Windows**: 524 xUnit cumulativi attesi. V14 predefinita e v13 legacy con SQLite separati. D47 unifica verifica fabbricazione Matite fra sessione/UI, messaggi quantitativi e 6 nuovi test; conserva 1 Legno lavorato + 1 Carbone → 4 Matite, almeno 2 Matite/studente, rimborso solo se realmente prepagate, vecchi save compatibili. Niente modifica dei coefficienti E4.

Prossima P1-D48 tecnologie/prerequisiti/benefici, D49 scenari integrati, D50 gate; poi E4a misure → E4b bilanciamento approvato → E4c consolidamento → M5.4–M7. Non attribuire test PASS non eseguiti; consegnare sempre ZIP intero. [Rapporto D47](design/P1_D47_RC33_FILIERA_MATITE.md) · [Piano RC33](validation/P1_D47_RC33_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Stato](CURRENT_STATE.md).

---

# HANDOFF PRIORITARIO RC32 — P1-D46

**Archivio da usare:** SimRisorse-v60-E3b4-RC32-P1-D46-formazione-individuale.zip. Ultimo checkpoint testato dall'utente: **RC30, 511 test previsti e passati**. RC31 (515) e RC32 (518) **ancora da verificare su Windows**, cumulativi. La GUI v14 e' ora predefinita, v13 soltanto con `--legacy-v13`; database separati. RC32 visualizza i progressi individuali nella Scuola (massimo 6 righe + conteggio ulteriori), senza cambiare regole/SQLite. P1-D47 prossimo: filiera Matite. Poi D48 tecnologie, D49 integrazione, D50 chiusura, E4a analisi, E4b valori approvati, E4c consolidamento e M5.4–M7.

Leggere [rapporto](design/P1_D46_RC32_CICLO_FORMAZIONE_SCUOLA.md), [piano test](validation/P1_D46_RC32_TEST_PLAN.md), [roadmap](ROADMAP.md) e [stato](CURRENT_STATE.md). Non dichiarare test superati prima dell'esito Windows; consegnare sempre ZIP integrale.

---

# SimRisorse — RC31 / G1-D45: v14 predefinita (candidata)

**Aggiornamento 04/11/2026:** la **RC30** ha superato i test secondo la conferma dell'utente (**511 test previsti**). La GUI v14 è stata dichiarata soddisfacente e diventa l'interfaccia ordinaria; lo storico dossier di **44 prove visive non acquisite** resta disponibile ma non è più prerequisito per avviare P1. **RC31** aggiunge quattro test di routing (515 attesi) e deve ancora essere compilata/testata su Windows.

Avvio: `.\run-game.cmd` → v14, sullo **stesso** slot SQLite v14 già usato (`%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`). `.\run-game.cmd --legacy-v13` → v13, DB storico separato. `--experimental-v14` funziona ancora come alias. `--scenario ...` e `--smoke-test` rimangono legacy per compatibilità. Nessuna migrazione automatica né modifica al bilanciamento.

**Prossima milestone:** P1-D46 audit ciclo Scuola; D47 Matite; D48 tecnologie; D49 integrazione; D50 gate. Quindi E4a → E4b → E4c → M5.4–M7. [ADR 0042](adr/0042-promozione-v14-e-conservazione-slot.md) · [Rapporto D45](design/G1_D45_RC31_PROMOZIONE_V14.md) · [Piano RC31](validation/G1_D45_RC31_TEST_PLAN.md).

---

# Nuova chat — SimRisorse RC30 / G1-D44

**Usare lo ZIP integrale RC30.** `docs/ROADMAP.md`, `docs/CURRENT_STATE.md`, `docs/HANDOFF.md`, `docs/design/G1_D44_RC30_RAFFERZAMENTO_EVIDENZE_P0.md`, `docs/validation/G1_D44_RC30_TEST_PLAN.md`.

> **Checkpoint Windows confermato:** RC27 con 507 test passati. RC28–RC30 sono **cumulative e non ancora validate** (511 test xUnit attesi). RC29 ha introdotto `scripts/p0-ui-evidence.ps1` per 44 scenari (22 UI-ID × 2 viewport). RC30 corregge i falsi positivi per PNG finti/corrotti e NA; aggiunge `-Action Verify` e `scripts/test-p0-ui-evidence.ps1`. Nessuna modifica al gioco o ai test xUnit. Screenshot reali qui: **0/44**; **P0 APERTO**. I 44 PASS formali non chiudono P0 da soli.
>
> **Rispettare** GUI v13 pixel art, tabella Risorse a 7 colonne, livelli Base/Grande/Avanzato distinti, edifici bloccati grigi e selezionabili, Costruisci/Lavoratore su due righe, Scuola/Matite, protezione salvataggi. E4 e navale restano sospesi fino ai gate. Sviluppare correzioni grafiche D44 solo sulla base di evidenze visive realmente raccolte. Consegna sempre ZIP completo e handoff.

---

# Handoff prioritario — G1-D43 / RC29 (strumenti per P0)

**Usare RC29, progetto integrale.** RC27: **507 test dichiarati superati**. RC28: 4 nuovi test focus, **511 attesi e non eseguiti**; RC29 non aggiunge altri test. Nessuna modifica all'applicazione rispetto a RC28, solo strumenti PowerShell/documenti di audit. **P0 visivo: 0/44 scenari registrati in questa consegna, aperto**. Non confondere verifica statica dei sorgenti con confronto di finestre eseguito su Windows.

Per la ripresa: `docs/ROADMAP.md`, `docs/validation/G1_D39_P0_UI_MATRIX.md`, `docs/design/G1_D43_RC29_AUDIT_VISIVO.md`, `docs/validation/G1_D43_RC29_PIANO_EVIDENZE.md`, `scripts/p0-ui-evidence.ps1` (inizializza CSV, salva immagine dagli Appunti solo su richiesta, crea HTML comparativo). Gli screenshot/CSV vanno tenuti in `.local-cache` fuori dallo ZIP. Non aggiornare a PASS senza prove. D44: fix solo difetti riproducibili; D45: gate P0. Poi P1 scuola/ricerca, E4a misure, E4b bilanciamento approvato, E4c migrazione, M5.4–M7. Preservare v13, 7 colonne Risorse, tier separati, Scuola/Matite, salvataggi distinti.

---

# Handoff prioritario — RC28 / G1-D42 (candidata)

**Riprendere dall'archivio integrale RC28.** L'utente ha confermato che **RC27 ha superato 507 test**. RC28 aggiunge uno stile di focus **solo alla finestra v14** e quattro esecuzioni di test Desktop headless, quindi **511 test attesi ma non eseguiti**. Nessuna modifica alla GUI classica, al motore, ai database o ai coefficienti economici.

**Prossime operazioni:** eseguire `.\validate.cmd` sulla RC28 e raccogliere evidenze focus/tastiera a 1040×740 e 1240×850; avviare G1-D43 sulle **22 voci P0** soltanto quando disponibili test e prove Windows. La matrice non è ancora approvata; non dichiarare la GUI certificata. D44 corregge difetti dimostrati, D45 gate P0; poi P1-D46…D50, E4a/E4b/E4c e M5.4–M7.

**Vincoli:** preservare GUI v13, sette colonne Risorse, selezione ocra, edifici grigi ma selezionabili, righe Costruisci/Lavoratore, Scuola/Matite, salvataggi isolati. Consegnare ZIP integrale e aggiornare i documenti di handoff.

[Rapporto D42](design/G1_D42_RC28_FOCUS_TASTIERA.md) · [Piano test](validation/G1_D42_RC28_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff prioritario — G1-D42 (documentazione) / RC27 validata

**Ripartire dal progetto integrale RC27 con aggiornamento documentale D42.** L'utente ha confermato **507 test passati** per RC27 il 04/11/2026. La revisione D42 definisce il piano, **non cambia codice, test né schema**; non esiste una RC28 per questo lavoro. **P0 grafico ancora aperto** per tutte le 22 verifiche visive alle risoluzioni 1040×740 e 1240×850, senza attribuire PASS non documentati.

**Ordine concordato:** G1-D42 focus/tastiera, G1-D43 prove visive UI-01…UI-22, G1-D44 fix mirati, G1-D45 gate → P1-D46…D50 Scuola/Matite/Albero → E4a misurazioni → E4b solo valori approvati → E4c migrazione protetta → M5.4–M7 navale/esplorazione/conquista. **Primo lavoro da avviare:** G1-D42 audit reale su focus, tastiera, scorrimento, selezioni, senza redesign. Usare `docs/ROADMAP.md`, `docs/validation/G1_D42_P0_RUNBOOK.md`, `docs/validation/G1_D39_P0_UI_MATRIX.md` e ADR 0040.

**Preservare:** GUI v13 e salvataggi v13 intatti; Risorse sette colonne, edifici distinti/righe Costruisci e Lavoratore, Scuola non costruibile dalla sua scheda, 2 Matite minime/studente; SQLite separati; ZIP integrale per ogni codice; numeri economici candidati fino a E4b.

---

# Nuova chat — SimRisorse RC27 / G1-D41

**Usare l’archivio integrale RC27**, non RC26. **RC26: 504 test superati dichiarati dall’utente.** RC27 corregge solo la v14 nel dettaglio Edifici: entrambi gli ScrollViewer tornano all'inizio quando cambia la selezione; la lettura non viene interrotta da aggiornamenti della stessa riga. 3 nuovi test Desktop, **507 attesi**, non eseguiti qui per SDK .NET 10 assente. Nessuna modifica alle logiche di gioco, GUI v13, SQLite o salvataggi. **P0 visivo ancora aperto**: 22 voci alle due risoluzioni; D42 per focus/tastiera e anomalie residue. Solo dopo P0 passare a P1, E4a/b/c e M5.4–M7.

[Rapporto](design/G1_D41_RC27_AUDIT_SCORRIMENTO_DETTAGLI.md) · [Test RC27](validation/G1_D41_RC27_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Matrice](validation/G1_D39_P0_UI_MATRIX.md).

---

# Nuova chat — SimRisorse RC26 / G1-D40

**Usare lo ZIP integrale RC26**, non i vecchi pacchetti. **RC25 confermata verde dall’utente (501/501)**. RC26 aggiunge 3 test Desktop (504 attesi), correggendo solo il testo v14 della crescita per recuperare ventesimi di progresso e ETA del prossimo abitante accelerato (×1,15), e protegge il tempo di riempimento UI-05 con test sul saldo netto. **Nessuna modifica a v13, Core, SQLite, XAML o coefficienti E4.** Prossima azione: `./validate.cmd` su RC26 quando possibile; confronto visivo UI-05/UI-14 e altre voci della [matrice P0](validation/G1_D39_P0_UI_MATRIX.md) alle risoluzioni 1040×740 e 1240×850. **P0 aperto**, poi P1, E4a/b/c e M5.4–M7.

[Rapporto D40](design/G1_D40_RC26_FLUSSI_POPOLAZIONE.md) · [Piano RC26](validation/G1_D40_RC26_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Nuova chat — SimRisorse RC25 / G1-D39-F1

**Usare l'archivio integrale RC25.** Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, la [matrice P0](validation/G1_D39_P0_UI_MATRIX.md) e il [piano test RC25](validation/G1_D39_RC25_TEST_PLAN.md).

> **Ultimo gate verde Windows:** RC20, 490 test. **RC24 fallita in compilazione** di Desktop.Tests per CS1061 su `window.MouseMove` (righe 71 e 136 del test upgrade). **RC25** corregge solo l'import `using Avalonia.Headless;` in `V14BuildingUpgradeAuditTests.cs`, senza alterare codice applicativo o le asserzioni. **501 test attesi, non ancora eseguiti**. RC25 include tutti gli incrementi RC21–RC24. Non attribuire test verdi fino all'esito Windows. P0 resta aperto: confronto delle 22 voci a 1040×740 e 1240×850. G1-D40 flussi/crescita, poi P1, E4a, E4b, E4c, M5.4–M7. Salvaguardare GUI v13, database distinti e ZIP integrali.

[Rapporto F1](design/G1_D39_RC25_FIX_CS1061_HEADLESS.md) · [Roadmap](ROADMAP.md) · [Piano Windows](validation/G1_D39_RC25_TEST_PLAN.md).

---

# Nuova chat — SimRisorse RC24 / G1-D39

**Allegare ZIP integrale RC24.** Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, la [matrice operativa P0](validation/G1_D39_P0_UI_MATRIX.md) e il [piano test RC24](validation/G1_D39_RC24_TEST_PLAN.md).

> **Gate confermato dall'utente:** RC20, 490 test passati. **RC21 493, RC22 496, RC23 499, RC24 501 attesi:** tutti non ancora testati su Windows, ma cumulativi in RC24. D39 ha corretto solo il testo dei suggerimenti Scuola (produrre Matite, iscrivere, annullare) durante «Nuova partita?» e aggiunto due test; niente nuove regole. Le 22 verifiche di parità richiedono ancora confronto manuale a 1040×740 e 1240×850: P0 APERTO. Prossima D40 audit UI-05/UI-14, poi P1 Scuola/Albero; E4a misurazioni → E4b bilanciamento approvato → E4c backup/migrazione → M5.4–M7. Preservare v13 e SQLite isolati; fornire sempre ZIP integrale, changelog, roadmap e handoff.

[Report D39](design/G1_D39_RC24_AUDIT_SCUOLA_E_GATE_P0.md) · [Roadmap](ROADMAP.md) · [Test Windows](validation/G1_D39_RC24_TEST_PLAN.md).

---

# Nuova chat — SimRisorse RC23 / G1-D38

**Allegare lo ZIP integrale RC23.** Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, la matrice `docs/validation/G1_D29_AUDIT_GUI_V13_V14.md` e il piano `docs/validation/G1_D38_RC23_TEST_PLAN.md`.

> **Stato:** RC20 ultima versione confermata 490 test; RC21 493 / RC22 496 / **RC23 499 test attesi ancora non eseguiti su Windows**. RC23 è cumulativa. D38 ha aggiunto tre test Desktop per navigazione a cinque voci e selezione dopo tick / Salva-Carica. Nessun codice applicativo cambiato. P0 visivo ancora aperto e non dichiarabile approvato. **D39:** proseguire audit comparativo UI-01…UI-22, completare prove visive 1040×740/1240×850 quando possibile e correggere soltanto difetti verificabili. Sequenza vincolante P0 → P1 → E4a → E4b → E4c → M5.4–M7. Consegnare sempre ZIP completo con changelog/roadmap/handoff/piano test.

[Stato](CURRENT_STATE.md) · [Report](design/G1_D38_RC23_AUDIT_NAVIGAZIONE_SELEZIONE.md) · [Test](validation/G1_D38_RC23_TEST_PLAN.md).

---

# Nuova chat — SimRisorse RC22 / G1-D37

**Allegare lo ZIP integrale RC22.** Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/validation/G1_D29_AUDIT_GUI_V13_V14.md` e `docs/validation/G1_D37_RC22_TEST_PLAN.md`.

> **Checkpoint Windows confermato:** RC20, 490 test dichiarati superati. **RC21** 493 attesi e **RC22** 496 attesi **NON ancora testati** dall'utente; non dichiararli verdi. RC22 include RC21.
>
> **D37:** un solo XAML v14: diagnosi Edifici con scrollbar verticale e altezza massima di 104 px, senza nascondere Costruisci/Lavoratore o cambiare i loro binding. 2 test headless alle viewport 1040×740 e 1240×850; +1 test UI per progresso Esercito a 10/30 secondi, **496 complessivi attesi**. Nessun cambiamento al motore, alle regole o ai salvataggi.
>
> **Prossima D38+**: test Windows cumulativi + revisione visiva delle 22 voci UI-01…UI-22 e correzioni reali; **P0 non ancora approvato**. Mantenere v13 intatta, tabella Risorse sette colonne, livelli distinti, Scuola/Matite, SQLite v2/v3 isolati. Roadmap P0 → P1 → E4a → E4b → E4c → M5.4–M7. Consegnare sempre ZIP completo, changelog, piano test, roadmap e handoff. **Nessun E4** prima del gate P0.

[Stato](CURRENT_STATE.md) · [Report D37](design/G1_D37_RC22_SCORRIMENTO_DETTAGLI_ESERCITO.md) · [Piano](validation/G1_D37_RC22_TEST_PLAN.md).

---

# Nuova chat — SimRisorse RC21 / G1-D36

**Allegare ZIP integrale RC21.** Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/validation/G1_D29_AUDIT_GUI_V13_V14.md` e `docs/validation/G1_D36_RC21_TEST_PLAN.md`.

> **Baseline Windows verde:** RC20, 490 test attesi e dichiarati superati. **RC21 non testata:** 493 attesi (+3). L'unica modifica applicativa è a `src/SimRisorse.Desktop/Views/V14PreviewWindow.axaml`: entrambi i pulsanti upgrade dispongono ora di `IsEnabled` esplicito e stile verde/ocra/grigio coerente con Costruisci. **Non nascondere** un upgrade tecnologicamente disponibile solo perché mancano risorse; mantenere visibili costi e deficit. Nuovi test `V14BuildingUpgradeAuditTests.cs` a due viewport e scenario di acquisto.
>
> **P0 ancora aperto:** prove visuali v13/v14 delle 22 voci a 1040×740 e 1240×850, scorrimento e dettagli Edifici ed Esercito. RC20 comprende già il gate RC17 dei checkpoint. Non introdurre nuove meccaniche né E4 ora.
>
> **Preservare** GUI classica v13, tabella Risorse a sette colonne, edifici bloccati grigi ma selezionabili, Base/Grande/Avanzato distinti, due righe Costruisci/Lavoratore, capacità Matite, SQLite v2/v3 separati; per ogni passaggio fornire ZIP completo, test, changelog, roadmap e handoff. Distinguere test Windows da verifiche statiche.

[Stato](CURRENT_STATE.md) · [Piano RC21](validation/G1_D36_RC21_TEST_PLAN.md) · [Rapporto](design/G1_D36_RC21_AUDIT_EDIFICI_UPGRADE.md).

---

# Nuova chat — SimRisorse RC20 / G1-D35

**Allegare il progetto integrale RC20.** Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/validation/G1_D29_AUDIT_GUI_V13_V14.md` e `docs/validation/G1_D35_RC20_TEST_PLAN.md`.

> Continua da RC20. **Ultimo gate Windows confermato: RC16, 476 test.** RC17 (481), RC18 (484), RC19 (487) e **RC20 (490 attesi)** non sono ancora state testate. RC20 include tutte le precedenti, senza cambiarne il codice applicativo.
>
> **D35:** audit statico della tabella Risorse: v14 conserva le sette colonne della v13, anche la scrollbar e il dato di capienza. Aggiunti 3 casi di test in `V14ResourcesAuditTests.cs` (due viewport Avalonia + identità/ordine/aggiornamento). Nessuna modifica arbitraria al layout. Eseguire un solo `./validate.cmd` su RC20 quando possibile, poi controllo visivo a 1040×740 e 1240×850. Non dichiarare P0 chiuso senza conferma visiva.
>
> **Priorità D36:** Edifici (grigi ma selezionabili, due righe Costruisci/Lavoratore, disabilitazione e hover, costo cumulativo, diagnosi). **RC17:** protezione Nuova partita/autosalvataggio: conferma Windows pendente, con salvataggio di prova.
>
> Roadmap: P0 grafico → P1 Scuola/tecnologie → E4a analisi → E4b bilanciamento → E4c consolidamento/migrazione → M5.4–M7. **Preservare v13 e SQLite**. Consegna sempre archivio integrale, changelog, roadmap, handoff e test. Separare prove statiche dai risultati Windows.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Piano RC20](validation/G1_D35_RC20_TEST_PLAN.md) · [Rapporto](design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md).

---

# Nuova chat — SimRisorse RC19 / G1-D34

**Allegare lo ZIP integrale RC19**. Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/validation/G1_D29_AUDIT_GUI_V13_V14.md` e `docs/validation/G1_D34_RC19_TEST_PLAN.md`.

> Continua da **SimRisorse RC19 / G1-D34**. Ultimo checkpoint Windows dichiarato superato: **RC16, 476 test**. RC17 (**481**), RC18 (**484**) e RC19 (**487 attesi**) non sono state testate dall'utente. RC19 include tutte le modifiche RC17+RC18. **Non dichiararle verdi** prima del prossimo `validate.cmd`.
>
> **RC19:** `V14PreviewViewModel.Parity.cs` rende coerenti i suggerimenti Esercito con il blocco dei comandi durante «Nuova partita?»; mostra disponibili/richiesti per Spade, abitanti, reclute annullabili e spazio rimborso; singolare «1 recluta». +3 test Desktop in `V14MilitaryUiParityTests.cs`, incluso Avalonia headless. Nessuna variazione delle regole militari, del motore, del layout XAML, della GUI v13 o SQLite.
>
> **RC18:** suggerimenti Scuola coerenti con il lotto +5/+10, posti e ricercatori qualificati. **RC17:** protezione del checkpoint v14 fra creazione nuova colonia, autosalvataggio e chiusura; **richiede ancora test Windows di sicurezza su copia del salvataggio**.
>
> **Roadmap confermata:** Fase 1 P0 audit GUI 22 voci, 1040×740/1240×850 (ora Risorse/Edifici prioritari e verifica Esercito); poi P1 Scuola/tecnologie, E4a misurazioni, E4b bilanciamento, E4c migrazione con backup, M5.4–M7 navale/esplorazione/conquista. No E4 prima del gate P0 e nessun cambio di coefficienti.
>
> **Metodo:** consegna ZIP integrale, changelog, roadmap, handoff e test a ogni incremento. Evidenze statiche e Windows sempre separate; preservare la v13, percorsi SQLite, dati personali. Il confronto grafico necessita verifica Windows reale, non è ancora approvato.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Piano RC19](validation/G1_D34_RC19_TEST_PLAN.md) · [Rapporto D34](design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md).

---

# Nuova chat — SimRisorse RC18 / G1-D33

**Allegare ZIP integrale RC18**. Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/validation/G1_D29_AUDIT_GUI_V13_V14.md` e `docs/validation/G1_D33_RC18_TEST_PLAN.md`.

> Continua da **SimRisorse RC18 / G1-D33**. Ultimo gate confermato su Windows: **RC16, 476 test passati**. RC17 (481 attesi) **non è stata testata**; RC18 include RC17 e ha 3 test aggiuntivi, **484 attesi**, ancora da verificare. Non dichiarare verdi RC17 o RC18 prima dell'esito Windows. L'utente ha esplicitamente chiesto di continuare anche in assenza temporanea di test.
>
> RC18 corregge il suggerimento della Scuola che presentava un lotto +5/+10 di ricercatori come assegnabile pur avendo meno posti liberi; distingue ora Scuola assente, posti insufficienti e qualificati insufficienti. Solo ViewModel v14, test, documentazione.
>
> RC17 protegge il checkpoint v14 durante «Nuova partita», salvataggi manuali e automatici; richiede verifica. Mai toccare GUI classica, file v13/SQLite, cataloghi economici o testimoni di una partita personale.
>
> Roadmap approvata: P0 audit GUI (22 voci, risoluzioni 1040×740 e 1240×850); P1 Scuola/tecnologie; E4a misure; E4b bilanciamento approvato; E4c migrazione con backup; M5.4–M7 navale/esplorazione/conquista. Prima validare RC17+RC18 quando possibile, poi Risorse/Edifici/Esercito. Test automatici non sostituiscono audit visivo.
>
> Ogni avanzamento: ZIP integrale, changelog, roadmap, handoff, piano test e ADR solo se decisioni architetturali nuove. Separare chiaramente verifiche statiche da test xUnit non eseguiti.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Piano RC18](validation/G1_D33_RC18_TEST_PLAN.md).
