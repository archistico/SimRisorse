# RC42 / F1-D03 — Ortofrutta e correzione urgente backup v14 (2026-11-04)

- **RC41 validata dall'utente: 563/563 test superati**. **RC42 candidata: 577 test attesi**, 14 nuovi, esecuzione Windows pendente.
- Corretto `V14SqliteSaveStore.BackupExisting`: dopo i 100 backup storici occupati crea backup aggiuntivi **senza limiti di slot**, con nome univoco e `VACUUM INTO`, conservando **tutti** i backup precedenti. Nessuna rotazione automatica; lo spazio disco può crescere. Due nuovi test Infrastructure su backup 101/102 e checkpoint invalidi.
- Introdotto `HorticultureModel.cs` Core **isolato**: Orto→Verdura e Frutteto→Frutta, personale e capienza indipendenti, uso facoltativo di Acqua configurabile e lotti atomici. 12 test Core. Nessun costo/ricetta definitiva approvata.
- **Gioco corrente invariato salvo il fix del salvataggio:** catalogo, GUI v14/v13, motore di produzione, schema SQLite e regole alimentari attive non modificati. Ortofrutta non ancora costruibile; collegamento al deposito e migrazioni in F1-D04/D05.
- Documentazione: [rapporto](docs/design/F1_D03_RC42_ORTOFRUTTA_E_BACKUP_V14.md) · [validazione](docs/validation/F1_D03_RC42_TEST_PLAN.md) · [ADR 0052](docs/adr/0052-backup-v14-oltre-cento-slot.md) · [ADR 0053](docs/adr/0053-orto-frutteto-modello-isolato.md) · [roadmap](docs/ROADMAP.md).

---

# RC41 / F1-D02 — Stalla, Cacciatore e Macellaio a ricette alternative (2026-11-04)

- RC40 **validata dall'utente, 552 test superati**; nuova RC41 **563 test attesi e non eseguiti qui**.
- Creato `src/SimRisorse.Core/Food/MeatChainModel.cs`: produzione distinta Grano+Acqua→Bovini, Cacciatore→Selvaggina e due ricette alternative Macellaio→Carne, con lotti interi, capienza, lavoratori condivisi, priorità configurabile e divieto di macellazione nello stesso tick degli animali appena prodotti.
- Aggiunti **11 test Core** in `MeatChainFoundationTests.cs`, incluso il passaggio obbligato Carne→Deposito→Cibo pronto e la verifica di assenza di sprechi. Rese dei test solo illustrative, **nessun nuovo coefficiente approvato**.
- **Dominio isolato:** nessuna attivazione delle tre nuove risorse o degli edifici nella v14; invariati la GUI, il catalogo della partita, le regole economiche, SQLite e i salvataggi. La migrazione e i nuovi tier restano in F1-D04/D05.
- Aggiornati [rapporto F1-D02](docs/design/F1_D02_RC41_FILIERA_CARNE.md), [piano test](docs/validation/F1_D02_RC41_TEST_PLAN.md), [ADR 0051](docs/adr/0051-due-origini-carne-e-priorita-macellaio.md), roadmap, stato corrente e handoff.

---

# RC40 / F1-D01 — Modello di deposito alimentare (2026-11-04)

- Recepita la riforma approvata: Pesce, Pane, Carne, Frutta e Verdura passano dal Deposito alimentare; Stalla e Cacciatore alimentano due vie verso il Macellaio; Orto e Frutteto sono filiere distinte. Pane necessario anche alle abitazioni avanzate, con riserva non convertibile due volte.
- Creato `SimRisorse.Core/Food/FoodDepotModel.cs`: inventario grezzo/Cibo pronto, ricezione atomica per capacità, trasformazione a lotti interi entro i limiti di personale e capienza, priorità deterministica e protezione della riserva Pane.
- **7 test Core aggiunti** in `FoodDepotFoundationTests.cs`; **552 test complessivi attesi**. Ultima conferma RC38: **541/541**; RC39 (545) e RC40 (552) da validare. Nessun benchmark E4a misurato.
- Implementazione **isolata**: il nuovo planner NON è ancora collegato a V14CampaignSession, GUI, catalogo, save SQLite o motore. Nessuna modifica ai parametri produttivi, al bilanciamento o ai salvataggi esistenti.
- Nuovi [modello F1-D01](docs/design/F1_D01_RC40_MODELLO_ALIMENTARE.md), [ADR 0050](docs/adr/0050-filiera-alimentare-unificata-deposito.md), [piano Windows](docs/validation/F1_D01_RC40_TEST_PLAN.md). Roadmap/handoff aggiornati; E4a storica resta utilizzabile solo come precedente economico, E4b rimandata.

---

# RC39 / E4a-D02 — Benchmark di progressione naturale (2026-11-04)

- **Baseline RC38:** 541 test Windows confermati superati dall'utente. **RC39 candidata:** +4 test, **545 attesi**, non eseguiti nel confezionamento.
- Aggiunto `E4aNaturalBenchmark` per E4A-01…05: stessa partita iniziale v14 non finanziata, politica `natural-reference-v1` dichiarata, eventi dei comandi, checkpoint periodici ogni 60 secondi simulati e obiettivi osservabili (Cibo, Pane/abitante avanzato, Scuola, tecnologia, Segheria Grande).
- Comando CLI `--benchmark-e4a-natural [--scenario E4A-0X] [--output DIRECTORY]`: JSON/CSV, prevenzione sovrascritture e esiti `pass`/`timeout`/`blocked` senza tempi inventati. Quattro test Application su stock iniziale, avanzamento reale, determinismo, formato e parametri.
- Documentazione E4a-D02, ADR 0049, piano Windows e handoff aggiornati. `benchmarks/e4a/scenarios.csv` resta **not-run**: nessun benchmark dichiarato misurato.
- **Invariati:** Core, GUI v13/v14, SQLite, salvataggi, costi, ricette e coefficienti. E4b subordinato ad analisi e approvazione.

---

# RC38 / E4a-D01-F1 — Confronto snapshot SQLite indipendente dall'ordine (2026-11-04)

- **Evidenza RC37 su Windows:** 541 test eseguiti, 540 superati e 1 fallito: `V14SchoolResearchEndToEndTests.PencilsToStudentToResearchToLargeTierSurvivesTwoSqliteRoundTrips`, per ordine di `OtherBuildings` diverso dopo caricamento SQLite.
- Corretto esclusivamente il confronto nel test: `OtherBuildings` per `BuildingId`, `ProductiveFamilies` e `PurchaseCounters` per `FamilyId`, `UnlockedTechnologyIds` per ID, sempre con `StringComparer.Ordinal`. Confronto **integrale** dei record e verifica ordinata della formazione invariati.
- Nessuna modifica a GUI, Core, Application, SQLite, save schema, ricette o benchmark; **541 test attesi, RC38 non ancora validata**.
- [Rapporto](docs/design/E4A_D01_RC38_FIX_ORDERING_SQLITE.md) · [Piano Windows](docs/validation/E4A_D01_RC38_TEST_PLAN.md).

---

# RC37 / E4a-D01 — Runner riproducibile per confronto Scuole (2026-11-04)

- Primo benchmark **v14 realmente eseguibile** nella CLI `SimRisorse.Simulation --benchmark-e4a-school`, scenari controllati E4A-06/07 (1 o 2 Scuole): stessa dotazione di partenza dichiarata, comandi veri, 30 s per campione, CSV/JSON senza tempi o checkpoint fittizi.
- Nuovo componente `SimRisorse.Application.Benchmarks.E4aSchoolBenchmark`: nessun cambiamento alle regole del gioco, ai coefficienti, alla GUI, ai salvataggi. Output non sovrascrivibile.
- **3 test nuovi** (esito/tempi, ripetibilità, CSV): **541 test cumulativi attesi; NON eseguiti qui.** Ultimo gate Windows confermato RC30 (511); RC31–RC37 ancora candidate.
- Protocollo, report, piano di validazione, ADR 0048 e handoff aggiornati. **E4a non ha ancora risultati misurati; E4b non iniziata.**
- [Rapporto E4a-D01](docs/design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) · [Test plan](docs/validation/E4A_D01_RC37_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

# RC36 / P1-D50 — Consolidamento Scuola/Ricerca e preparazione E4a (2026-11-04)

- Aggiunti **2 test** a `V14SchoolPencilPersistenceTests.cs`: salvataggio SQLite pre-Matite con uno studente realmente in formazione e assenza fisica della colonna `pencil_paid_students`; rifiuto del ledger Matite impossibile, senza sovrascrivere il checkpoint valido.
- Creati registro dei 10 scenari E4a (`natural`/`controlled`), template risultati e protocollo di misurazione; **non eseguiti**, nessun dato prestazionale dichiarato.
- Aggiornati piano test, roadmap, stato, handoff e ADR 0047. **Nessuna modifica al codice applicativo, GUI, costi, ricette, ricerca o formato SQLite.**
- **538 test cumulativi attesi; non eseguiti qui** (RC35: 536). Ultimo gate Windows confermato: RC30 (511). Gate P1 ancora aperto.

---

## RC35 / P1-D49 — Verifica end-to-end scuola, ricerca e tier (2026-11-04)

- Aggiunti tre test di integrazione `V14SchoolResearchEndToEndTests` su sessione v14 e SQLite v3: filiera Matite, iscrizione, 120 s formazione, assegnazione, accumulo 100 punti, acquisto Lavorazioni I, upgrade Segheria Grande, due round-trip e ripresa in pausa.
- Coperti anche formazione parziale e fallimenti atomici senza mutazioni; fixture con fondi entro capacità, nessun punto ricerca o tecnologia conferiti artificialmente.
- **536 test cumulativi attesi (533 + 3), non eseguiti qui**. Ultima conferma utente RC30 511; RC31–RC34 ancora candidate.
- **Nessuna modifica ai sorgenti dell'applicazione**, alla GUI v13/v14, al DB SQLite, a ricette o costi. Prossimo gate: P1-D50, dossier di consolidamento prima di E4a.
- [Rapporto](docs/design/P1_D49_RC35_INTEGRAZIONE_PROGRESSIVA.md) · [Validazione](docs/validation/P1_D49_RC35_TEST_PLAN.md) · [ADR 0046](docs/adr/0046-catena-integrata-scuola-ricerca-tier.md).

## RC34 / P1-D48 — Benefici effettivi dell'Albero tecnologico (2026-11-04)

- Introdotto `TechnologyTierEffects`: proiezione da regole tier concrete, verifica di copertura dichiarata e dipendenze Grande→Avanzato, rifiuto di ricerche acquistabili senza effetto.
- La sessione v14 valida il catalogo prima di accettare una nuova partita o un caricamento. Il dettaglio della ricerca ora mostra esplicitamente gli edifici Grande/Avanzato sbloccati; Logistica e Navigazione rimangono non disponibili.
- **9 test aggiunti, 533 cumulativi attesi; compilazione/xUnit pendenti su Windows.** Baseline RC30 (511 test confermati). RC31–RC33 incluse cumulativamente.
- Nessuna modifica a costi, produzione, personale, Matite, GUI v13, salvataggi o schema SQLite.
- [Rapporto](docs/design/P1_D48_RC34_ALBERO_TECNOLOGICO.md) · [Piano](docs/validation/P1_D48_RC34_TEST_PLAN.md) · [ADR 0045](docs/adr/0045-tecnologie-effetti-verificati.md).

## RC33 / P1-D47 — Filiera Matite e stato comandi (2026-11-04)

- Unificata la verifica della produzione Matite in `V14CampaignSession.CheckPencilCraft`: stessa valutazione del comando reale, senza modifiche durante la consultazione.
- Scuola v14: quantità mancanti di Legno lavorato e Carbone per +1/+5/+10, spazio Matite disponibile/necessario e pulsante coerente col dominio.
- Rafforzata l'atomicità dei lotti tramite test; test per annullamenti misti legacy/prepagati. Nessuna variazione a ricette, rimborsi, capacità, economia, SQLite o v13.
- **6 nuovi test (524 cumulativi attesi), da validare su Windows.** RC30 è l'ultimo checkpoint testato (511 confermati); RC31 e RC32 restano candidate incluse nella RC33.
- [Rapporto](docs/design/P1_D47_RC33_FILIERA_MATITE.md) · [Piano test](docs/validation/P1_D47_RC33_TEST_PLAN.md) · [ADR 0044](docs/adr/0044-unica-verifica-laboratorio-matite.md).

## RC32 / P1-D46 — Scuola: progressi individuali (2026-11-04)

- Aggiunto nella Scuola v14 un riepilogo di sola lettura degli avanzamenti individuali (fino a 6 posti con altri aggregati), aggiornato a ogni tick/caricamento.
- Conservati la barra del primo completamento e i comandi esistenti; invariati formazione, Matite, ricercatori, costi e SQLite.
- Aggiunti 3 test: UI binding headless, ripresa di due progressi differenziati con coda e test Core di formazione parallela.
- RC30 confermata 511 test; RC31 (515) non testata; RC32 **518 attesi e non testati**. Si richiede validazione Windows cumulativa.
- Documenti: [Rapporto](docs/design/P1_D46_RC32_CICLO_FORMAZIONE_SCUOLA.md), [Piano](docs/validation/P1_D46_RC32_TEST_PLAN.md), [ADR 0043](docs/adr/0043-dettaglio-individuale-formazione-scuola.md).

## 2026-11-04 — G1-D45 / RC31 — Promozione v14 a GUI predefinita (candidata)

- Dopo la conferma dell'utente sulla RC30 (511 test), la GUI v14 diventa la modalità **normale** all'avvio senza argomenti; v13 disponibile come `--legacy-v13`.
- `--v14` e `--experimental-v14` rimangono alias v14; `--scenario` e `--smoke-test` preservano il routing legacy degli script esistenti.
- Nessun trasferimento dei checkpoint: la v14 utilizza lo **stesso** file `experiments/civil-growth-v14.sqlite3`; protezione dei percorsi riservati contro aperture incrociate v13/v14.
- Eliminati i testi «sperimentale» dai banner della v14; struttura grafica e caratteristiche del gioco invariate. Aggiunti **4 test xUnit** per il routing (totale **515 attesi**, non ancora eseguiti qui).
- **Accettazione della GUI comunicata dall'utente**, non confondere con il dossier P0 di 44 screenshot mai acquisiti. ADR 0042, rapporto D45, piano RC31 e handoff aggiornati. Prossimo P1-D46 Scuola; E4 successivo.

---

## 2026-11-04 — G1-D44 / RC30 — Robustezza raccolta evidenze P0

- `scripts/p0-ui-evidence.ps1`: PNG controllati per contenuto leggibile, formato e dimensioni compatibili con viewport; false evidenze segnalate come `EVIDENZA_MANCANTE`.
- Le eccezioni `NA` non fanno risultare completata l'intera matrice; introdotta azione `Verify` con codici uscita 0/2 e messaggio di gate sempre subordinato ad approvazione manuale.
- Nuovo auto-test PowerShell autonomo `scripts/test-p0-ui-evidence.ps1`, rapporto D44, piano di validazione e ADR 0041.
- **Nessun cambiamento** a sorgenti dell'app, UI v13/v14, economia, salvataggi o ai 511 test .NET della candidata.
- **Baseline confermata:** RC27 507 test; RC28–RC30 511 attesi e **non eseguiti qui**; P0 visivo senza evidenze reali, aperto.

---

## v60-E3b4-RC29 / G1-D43 — Preparazione confronto visivo P0 (CANDIDATA, 04/11/2026)

- RC27 rimane l'ultima suite confermata dall'utente: **507 test**. RC28 introduce +4 test headless (511 attesi); **RC29 mantiene invariati 511 test, non eseguiti** qui.
- Creato `scripts/p0-ui-evidence.ps1`: inizializza registro da **44 scenari** senza sovrascrivere prove esistenti, opzionalmente salva PNG espliciti dalla clipboard Windows, e genera report HTML offline affiancando v13 e v14. Il report segnala evidenze mancanti e non chiude mai automaticamente P0.
- Audit statico delle due finestre: dimensioni condivise, sidebar 240, stessa griglia Risorse 7 colonne; differenza Edifici 310/410 e sezioni nuove Scuola/Albero; nessuna prova visiva reale disponibile. Documenti G1-D43 e handoff aggiornati. **P0 APERTO, 0/44 scenari visivi eseguiti qui.**
- Sorgenti applicativi, test xUnit, GUI v13, SQLite, costi, capacità e ricette **immutati** rispetto a RC28. Nessuna pretesa di build/test Windows o di prova runtime PowerShell. E4 sospeso.

---

## v60-E3b4-RC28 / G1-D42 — Focus e tastiera v14 (CANDIDATA, 04/11/2026)

- **Baseline valida Windows:** RC27, **507 test superati** confermati dall'utente. **RC28:** +4 esecuzioni Desktop headless (2 scenari, due risoluzioni), **511 test attesi**, non ancora compilati/testati in questo ambiente.
- Inseriti solo nella finestra v14 stili di **focus visibile** per menu laterale e liste Edifici/Risorse/Tecnologie. Bordo a larghezza costante (trasparente → ocra quando focalizzato); colori di selezione e hover immutati, nessun salto di layout da bordo di focus.
- Test di focus, frecce del menu, sincronizzazione delle schede, colore di selezione e geometria alle viewport 1040×740 e 1240×850. TAB/Shift+TAB e confronto visivo P0 restano da verificare realmente su Windows.
- Invariati GUI v13, Core, Application, SQLite v2/v3, ricette, prezzi, Matite e salvataggi; E4 non avviato. [Rapporto](docs/design/G1_D42_RC28_FOCUS_TASTIERA.md) · [Piano](docs/validation/G1_D42_RC28_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## G1-D42 — 04/11/2026 — Roadmap dettagliata post-RC27 (solo documentazione)

- **RC27 validata dall'utente:** 507 test superati su Windows; resta la stessa baseline applicativa, **nessuna RC28** e nessun nuovo test.
- `docs/ROADMAP.md` riscritta come piano canonico per D42–D45 (P0), P1-D46–D50, E4a, E4b, E4c e M5.4–M7; precedente roadmap storicizzata in `docs/history/operational/ROADMAP_G1_D41_RC27_ARCHIVE.md`.
- Nuovo `docs/validation/G1_D42_P0_RUNBOOK.md` per evidenze UI-01…UI-22, due viewport, focus, stato, salvataggi e gating; nuovo ADR 0040 sulla disciplina dei gate.
- Allineati README, START_HERE, CURRENT_STATE e handoff. **Nessun sorgente, test, schemi SQLite, salvataggio o coefficiente economico modificato.** P0 grafico non dichiarato chiuso.

---

## v60-E3b4-RC27 / G1-D41 — Reset scorrimento sul cambio edificio (CANDIDATA, 04/11/2026)

- **RC26 verde su Windows, 504 test superati**, confermati dall’utente. RC27: tre nuovi casi Desktop headless, **507 attesi**, non compilati/eseguiti qui per assenza di .NET 10.
- **UI-07/UI-12/UI-18:** reset degli offset del dettaglio principale e della diagnostica v14 al `SelectionChanged` dell’elenco Edifici. Refresh e aggiornamenti dello stesso edificio non causano reset, nessun trasferimento forzato del focus.
- Lo XAML aggiunge soltanto un nome al viewer principale; evento XAML collegato al code-behind; nuovi test a 1040×740 e 1240×850 più conservazione della posizione sullo stesso edificio.
- **Invariati:** GUI v13, disposizione pulsanti, tabella Risorse, economica/ricerca, motore, SQLite, salvataggi e ricette. Gate P0 aperto, E4 sospeso.
- [Rapporto D41](docs/design/G1_D41_RC27_AUDIT_SCORRIMENTO_DETTAGLI.md) · [Piano](docs/validation/G1_D41_RC27_TEST_PLAN.md).

## v60-E3b4-RC26 / G1-D40 — Presentazione crescita e audit flussi (CANDIDATA, 04/11/2026)

- **RC25 confermata su Windows dall’utente:** 501 test passati. **RC26:** +3 test Desktop, **504 attesi**, build e xUnit non eseguiti nell’ambiente di confezionamento.
- **UI-14:** nella v14 il testo di crescita perdeva i ventesimi di progresso e l’ETA del prossimo abitante dopo l’accelerazione; riutilizzata la logica di formattazione della GUI v13 con `GrowthActiveStatus`, tempo residuo arrotondato per eccesso e precisione 0,05 s. I blocchi Cibo/Pane/abitazioni conservano la precedenza.
- **UI-05:** test di regressione della stima `si riempie tra` basata su produzione meno consumo, con casi pieno, senza capienza, saldo nullo/negativo e tempi lunghi.
- **Invariati:** GUI v13, XAML, motore, regole E4, SQLite e salvataggi; P0 grafico delle 22 voci ancora aperto.
- [Rapporto](docs/design/G1_D40_RC26_FLUSSI_POPOLAZIONE.md) · [Piano](docs/validation/G1_D40_RC26_TEST_PLAN.md) · [Matrice](docs/validation/G1_D39_P0_UI_MATRIX.md).

## RC25 / G1-D39-F1 — 2026-11-04 — Correzione compilazione test CS1061

- **RC24:** compilazione `SimRisorse.Desktop.Tests` fallita prima dei test per `CS1061` su `V14PreviewWindow.MouseMove` (righe 71 e 136 di `V14BuildingUpgradeAuditTests.cs`).
- **RC25:** aggiunto `using Avalonia.Headless;` al file test; `MouseMove` è un metodo di estensione già usato con lo stesso namespace in altri test.
- **Nessun nuovo test:** 501 attesi, esecuzione Windows pendente; ultima suite confermata RC20, 490 passati. Nessuna modifica a codice applicativo, GUI, SQLite, prezzi o salvataggi; P0 visuale ancora aperto.
- Documentazione: [report](docs/design/G1_D39_RC25_FIX_CS1061_HEADLESS.md), [piano test](docs/validation/G1_D39_RC25_TEST_PLAN.md), roadmap e handoff aggiornati.

## v60-E3b4-RC24 / G1-D39 — Coerenza suggerimenti Scuola e matrice gate P0 (CANDIDATA, 04/11/2026)

- **Baseline confermata Windows:** RC20, 490 test. **RC21/RC22/RC23:** 493/496/499 attesi, non verificati singolarmente. **RC24:** +2 test Desktop, **501 attesi**, da validare cumulativamente su Windows.
- **D39-01 / UI-16, UI-20:** durante la conferma «Nuova partita?» i comandi iscrizione, annullamento studenti e produzione Matite erano bloccati, ma i messaggi della Scuola non lo indicavano. I tre suggerimenti ora mostrano «sospeso/a/i» e tornano allo stato precedente con Annulla.
- Nuovi test ViewModel + Avalonia: `V14SchoolConfirmationHintsAuditTests.cs`. Non cambiati costo 2 Matite/studente, simulazione, GUI v13, XAML, SQLite, salvataggi o bilanciamento.
- **D39-02:** nuova [matrice operativa delle 22 verifiche P0](docs/validation/G1_D39_P0_UI_MATRIX.md), con evidenza test/stato e controlli visivi 1040×740 e 1240×850 ancora da registrare.
- [Rapporto D39](docs/design/G1_D39_RC24_AUDIT_SCUOLA_E_GATE_P0.md) · [Piano Windows](docs/validation/G1_D39_RC24_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md). **P0 aperto, E4 sospeso.**

---

## v60-E3b4-RC23 / G1-D38 — Audit navigazione e selezione (CANDIDATA, 04/11/2026)

- **Ultima baseline Windows confermata:** RC20 (490 test). RC21 (493), RC22 (496), RC23 (**499 attesi**) non ancora eseguite su Windows.
- **UI-01/UI-19:** 3 nuovi test Desktop in `V14NavigationSelectionAuditTests.cs` (due viewport sidebar/schede/tick e uno Save/Load + identità selezioni); nessun bug di layout certo rilevato, pertanto nessuna modifica arbitraria a GUI o gameplay.
- **Invariati:** XAML v13 e v14, Core/Application/Infrastructure, SQLite e salvataggi, coefficienti. P0 visivo resta aperto; E4 sospeso.
- [Rapporto D38](docs/design/G1_D38_RC23_AUDIT_NAVIGAZIONE_SELEZIONE.md) · [Piano test](docs/validation/G1_D38_RC23_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## v60-E3b4-RC22 / G1-D37 — Scorrimento diagnosi Edifici e audit progressi Esercito (CANDIDATA, 03/11/2026)

- **Baseline verde confermata:** RC20, 490 test. **RC21 ancora non testata** (493 attesi); RC22 cumulativa, **496 test attesi** (+3), da validare su Windows.
- **Correzione GUI v14:** diagnosi produttiva nel box Edifici con scroll indipendente e altezza massima di 104 px; testi invariati, comandi costruzione/lavoratori e upgrade protetti dalla compressione verticale. La descrizione resta scrollabile a parte.
- **Test:** 2 casi Avalonia a 1040×740 e 1240×850 con diagnosi volutamente lunga, controllo di visibilità/scroll e selezione; 1 nuovo caso Avalonia sui progressi delle reclute dopo 10/30 secondi.
- **Ambito:** XAML v14 + test Desktop + documentazione. GUI classica v13, Core, economia, SQLite, schemi e salvataggi invariati. Nessun E4.
- **Gate:** ZIP/XAML verificabili staticamente, build .NET 10 e xUnit **non eseguiti qui**; P0 grafico resta aperto.
- [Rapporto D37](docs/design/G1_D37_RC22_SCORRIMENTO_DETTAGLI_ESERCITO.md) · [Piano RC22](docs/validation/G1_D37_RC22_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## v60-E3b4-RC21 / G1-D36 — Stati espliciti degli upgrade Edifici (CANDIDATA, 03/11/2026)

- **RC20 validata dall'utente:** 490 test attesi, tutti dichiarati passati. **RC21:** +3 casi Desktop, **493 attesi**; non compilati/eseguiti qui per assenza SDK .NET 10.
- **D36-01 / UI-10/UI-12/UI-18:** `V14UpgradeNext` e `V14UpgradeAdvanced` si affidavano solo a `CanExecute`, senza il binding `IsEnabled` e lo stile coerente con Costruisci. Ora sono grigi se non pagabili; verde + hover ocra solo quando pagabili, senza salti geometrici. Potenziamenti sbloccati ma non pagabili **restano visibili** con costo e carenze.
- **Tre test**: 1040×740, 1240×850, aggiornamento dinamico della disponibilità dal costo esatto e upgrade atomico Base→Grande.
- **Ambito ristretto:** solo XAML v14, nuovo file test, documentazione. Invariati GUI v13, Core, Application, Infrastructure, prezzi, SQLite e salvataggi. **Audit visivo P0 ancora aperto**. E4 sospeso.
- [Rapporto D36](docs/design/G1_D36_RC21_AUDIT_EDIFICI_UPGRADE.md) · [Piano RC21](docs/validation/G1_D36_RC21_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## v60-E3b4-RC20 / G1-D35 — Audit e test di non-regressione Risorse (CANDIDATA, 03/11/2026)

- **Ultima conferma Windows:** RC16 con 476 test. RC17 (481), RC18 (484), RC19 (487) non testate. **RC20:** 490 test attesi (+3), nessuna esecuzione .NET in questo ambiente.
- Il confronto XAML/VM con v13 mostra sette colonne, margine scrollbar e righe stabili. **Nessuna regressione riproducibile dimostrata dal codice:** evita modifiche arbitrarie al layout già approvato.
- +3 casi test: due viewport Avalonia headless (1040×740 / 1240×850), sei colonne numeriche allineate a destra, gutter, selezione ocra, tooltip e aggiornamenti; un test ordine alfabetico italiano/17 risorse/identità righe e dettagli Matite.
- **Nessuna modifica ai sorgenti dell'applicazione**: GUI v13/v14, cataloghi, economia, SQLite e salvataggi immutati. P0 visivo aperto, E4 sospeso. Prossimo G1-D36 Edifici.
- [Rapporto D35](docs/design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md) · [Piano Windows](docs/validation/G1_D35_RC20_TEST_PLAN.md) · [Matrice](docs/validation/G1_D29_AUDIT_GUI_V13_V14.md).

---

## v60-E3b4-RC19 / G1-D34 — Audit suggerimenti Esercito (CANDIDATA, 03/11/2026)

- **Ultimo gate Windows verificato:** RC16, 476 test. RC17 (481), RC18 (484) non testate; **RC19 attende 487 test** (+3 Desktop) e include integralmente le due candidate precedenti.
- **UI-15/UI-20:** nell'Esercito i suggerimenti di arruolamento, annullamento e limiti non potevano riconoscere la conferma «Nuova partita?» e potevano annunciare un comando possibile ma disabilitato. Ora indicano la sospensione temporanea e tornano coerenti dopo Annulla.
- Le carenze di Spade, abitanti, reclute rimborsabili e spazio di deposito riportano lotto, quantità disponibili e richieste. Corretto anche «1 recluta» al singolare. **Nessuna modifica ai comandi militari o al gameplay.**
- +3 test Desktop (stati, lotti e binding Avalonia); nessun cambiamento XAML, GUI v13, Core, SQLite, catalogo o salvataggi.
- [Rapporto D34](docs/design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Piano RC19](docs/validation/G1_D34_RC19_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md). **P0 visivo aperto, E4 sospeso.**

---

## v60-E3b4-RC18 / G1-D33 — Coerenza suggerimenti Scuola e lotti ricercatori (CANDIDATA, 03/11/2026)

- **Stato confermato:** RC16, 476 test Windows dichiarati superati dall'utente. **RC17 non ancora testata** (481 attesi); su richiesta dell'utente sviluppo proseguito. **RC18 candidata**, 3 nuovi test Desktop, **484 attesi**, suite .NET non eseguita in questo ambiente.
- **Difetto UI-16:** con Scuola e qualche posto di ricerca ancora disponibile, il suggerimento indicava «Assegna» anche con lotto +5/+10 superiore ai posti liberi, nonostante il comando fosse correttamente disabilitato.
- **Soluzione:** suggerimento ora confronta i posti ricerca disponibili con la quantità selezionata, distingue Scuola mancante, insufficienza di posti e insufficienza di ricercatori qualificati. Non cambia la possibilità di assegnare né i controlli del comando.
- **Test:** verifica del lotto +5 con 2 posti, cambio a +1 seguito da assegnazione effettiva, Scuola assente, e 6 posti con un solo qualificato. Stessi stati di errore e di abilitazione nei test. Nessuna modifica a GUI v13, XAML, Core, economia, SQLite o salvataggi.
- **Audit:** G1-D33 primo controllo aggiuntivo UI-16; UI-02/05/07/15 e verifica visiva 1040×740 / 1240×850 ancora aperti. E4 sospeso.
- [Rapporto D33](docs/design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Test RC18](docs/validation/G1_D33_RC18_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## v60-E3b4-RC17 / G1-D32 — Conferma nuova partita e autosalvataggio protetto (CANDIDATA, 03/11/2026)

- **Gate RC16:** l’utente ha comunicato **476 test superati**. RC17 aggiunge **5 test Desktop, 481 attesi**; compilazione/test Windows RC17 ancora pendenti.
- **UI-20/UI-21:** durante la conferma «Nuova partita?» ora vengono disabilitati anche Salva v14, Carica v14 e l'apertura di una seconda conferma; metodi pubblici Save/Load rifiutano l’operazione senza modificare la sessione.
- **Protezione del checkpoint:** dopo «Avvia nuova» l'autosalvataggio a intervalli e in chiusura resta disabilitato finché il giocatore non salva manualmente; conferma non ancora risolta + chiusura significa annullamento e salvataggio della vecchia colonia. Un caricamento riuscito ripristina il regime di autosalvataggio.
- Nuovo messaggio esplicito sull'autosalvataggio sospeso, nessuna modifica a GUI v13, Core, SQLite, schema o coefficienti.
- +5 test, ADR 0039, rapporto G1-D32 e piano Windows; **P0 visivo ancora aperto, E4 sospeso**.
- [Rapporto](docs/design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md) · [Piano test](docs/validation/G1_D32_RC17_TEST_PLAN.md) · [ADR](docs/adr/0039-protezione-checkpoint-e-autosalvataggio-v14.md).

## v60-E3b4-RC16 / G1-D31 — Blocco delle azioni durante «Nuova partita» (CANDIDATA, 03/11/2026)

- **Gate RC15:** l'utente ha dichiarato superati i test Windows (**473 attesi**). RC16 **476 test attesi**, non compilati/eseguiti nell'ambiente di confezionamento privo di .NET 10.
- **Difetto D31-01 (UI-20):** nella v14 la conferma «Nuova partita?» bloccava Pausa/Riprendi, ma lasciava abilitati Costruisci, Lavoratore, ricerca, Scuola, Matite ed Esercito. La v13 proteggeva le azioni di gioco durante la conferma.
- **Soluzione:** guardia unica `CanMutateColony` su tutte le azioni mutanti, anche su comandi legacy non esposti nel layout, con `CanExecute` aggiornato, disabilitazione esplicita delle azioni che usano binding `IsEnabled` e doppia protezione in `Perform`. Conferma e annullamento rimangono utilizzabili; cancellazione della conferma ripristina i comandi senza alterare scorte. Salva/Carica conservano il comportamento della v13; flusso da riesaminare separatamente in audit UI-20/UI-21.
- **Test:** +3 Desktop (no mutazioni/reversibilità, ricerca già acquistabile, stato visivo Avalonia); **476 attesi**. **Nessuna modifica** a GUI v13, XAML, Core, economia, SQLite, salvataggi o coefficienti. P0 grafico e E4 ancora aperti.
- [Rapporto D31](docs/design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](docs/validation/G1_D31_RC16_TEST_PLAN.md) · [Audit UI-01…UI-22](docs/validation/G1_D29_AUDIT_GUI_V13_V14.md).

## v60-E3b4-RC15 / G1-D30 — Parità capienza Matite e conferma nuova partita (CANDIDATA, 03/11/2026)

- **RC14: 470 test dichiarati superati dall’utente.** La GUI P0 resta da confrontare visivamente su Windows alle due risoluzioni previste.
- **UI-04/UI-12:** dettagli Edifici v14 ora elencano la capienza Matite di Municipio, Magazzino e Scuola con la stessa icona e lo stesso componente delle altre risorse. Per i primi due l’importo è ricavato dallo storage di Legno lavorato, per Scuola dalla costante Core nominata; valori economici effettivi invariati.
- **UI-20:** durante la conferma «Nuova partita» il comando Pausa/Riprendi è disabilitato come in v13 e torna disponibile alla chiusura della conferma.
- **Test:** +3 Desktop (dettagli capacità, guardia comando, disabilitazione visuale Avalonia), **473 attesi**; non eseguibili qui senza SDK .NET 10.
- **Ambito:** v14 presentazione e 1 costante nominata nel Core v14 senza cambiare la formula, test e documentazione. Nessun cambiamento a v13, XAML, SQLite, economie o salvataggi.
- [Report D30](docs/design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md) · [Piano RC15](docs/validation/G1_D30_RC15_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

# Changelog — SimRisorse

## v60-E3b4-RC14 / G1-D29-F1 — Fix compilazione tooltip Matite (CANDIDATA, 03/11/2026)

- **Risultato RC13 su Windows:** compilazione bloccata in `SimRisorse.Desktop` con **2 CS0136** (`owned`, `free` in `V14PreviewViewModel.Parity.cs`); suite xUnit non avviata. Ultimo gate Windows verde: RC12, **468 test attesi**, dichiarati passati dall'utente.
- **Causa:** nomi locali dichiarati nel ramo Matite che confliggono con nomi utilizzati nell'ambito contenitore dello stesso metodo C#.
- **Fix esclusivamente lessicale:** `owned` → `ownedBuilding` e `free` → `freePencilSpace` nel ramo tooltip Matite. Nessun cambiamento a formule, testi visibili, condizioni o comportamento.
- **Ambito:** un solo file C# di presentazione v14, più documentazione. Test invariati; **470 attesi RC14, da eseguire su Windows**. GUI v13/v14, Core, SQLite e salvataggi invariati. Audit grafico P0 e bilanciamento E4 ancora aperti.
- [Diagnosi](docs/design/G1_D29_RC14_FIX_CS0136.md) · [Piano RC14](docs/validation/G1_D29_RC14_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

## v60-E3b4-RC13 / G1-D29 — Roadmap in sei fasi, audit GUI e tooltip Matite (CANDIDATA, 03/11/2026)

- **Gate RC12:** test dichiarati tutti passati dall'utente (**468 previsti**, conteggio TRX effettivo non comunicato). La GUI P0 v13/v14 **non è ancora certificata visivamente**.
- **Roadmap riscritta:** sequenza P0 GUI → Scuola/tecnologie → E4a benchmark → E4b bilanciamento approvato → E4c migrazione/backup → M5.4–M7 espansione navale; stop rule e gate espliciti.
- **Audit iniziale D29:** inventario UI-01…UI-22 con evidenze, difetti e prove Windows ancora necessarie; nessun gate marcato verde senza verifiche.
- **Correzione presentazione v14:** tooltip della capacità Matite con effettivi Municipio, Magazzini e Scuole posseduti, quantità e spazio libero, coerente col calcolo Core. Niente bonus fissi attribuiti a edifici non costruiti.
- **Test:** +2 casi Desktop, iniziale e checkpoint con 2 Magazzini + 1 Scuola e selezione persistente dopo Load: **470 attesi**, non eseguiti qui per assenza SDK .NET 10.
- **Ambito:** un solo file di sorgente Desktop v14 e un test Desktop; v13, XAML, Core, Application, SQLite, ricetta Matite e salvataggi invariati. [Roadmap](docs/ROADMAP.md) · [Audit](docs/validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Piano RC13](docs/validation/G1_D29_RC13_TEST_PLAN.md) · [ADR 0038](docs/adr/0038-roadmap-gate-gui-e-audit-matite.md).

## v60-E3b4-RC12 / G1-D28 — Albero tecnologico: punti e stato Acquista (CANDIDATA, 03/11/2026)

- **Gate RC11:** test dichiarati tutti passati dall’utente (**466 attesi**; numero effettivo non comunicato). La parità visiva P0 non è ancora approvata.
- **Punti ricerca:** saldo, velocità di produzione e costi ricerca mostrati con due decimali `it-IT` (`N2`); valore interno invariato in millipunti (precisione 1/1000).
- **Acquista ricerca:** corretto il comando che verificava solo la selezione. Ora rifiuta all'interfaccia tecnologie già acquistate, non implementate, prive di prerequisiti/edifici o con saldo insufficiente. Pulsante `IsEnabled` esplicito, grigio in disabled e verde soltanto quando acquistabile; hover ocra e geometria invariati.
- **Test:** 2 nuovi casi Desktop (saldo 99,999 vs costo 100,000, acquisto esatto e stato post-acquisto; stile Avalonia grigio/verde/hover), **468 attesi**, non eseguiti qui per assenza SDK .NET 10. Verificata staticamente struttura XAML e diff circoscritto.
- **Ambito:** solo presentazione/XAML v14, nuovo selettore di stile per il pulsante ricerca, test Desktop e documentazione. Campagna v13, motore, salvataggi SQLite e Scuola/Matite non modificati.
- [Rapporto G1-D28](docs/design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano test](docs/validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](docs/adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).

## v60-E3b4-RC11 / G1-D27 — Comandi Edifici in due righe (CANDIDATA, 03/11/2026)

- **Gate RC10:** test dichiarati tutti passati dall’utente, **464 attesi**, numero effettivo non specificato; verifica grafica P0 ancora da completare.
- **Dettaglio Edifici v14:** label **Costruisci** e pulsanti **+1 / +5 / +10** sulla stessa riga; sotto label **Lavoratore** e pulsanti **+1 / +5 / +10**. L’azione preesistente **−1** resta accanto alla label Lavoratore per evitare una regressione funzionale.
- **Nuovi comandi:** assegna 5/10 lavoratori attraverso l’API di sessione già esistente, in un’unica operazione. Abilitazione solo se ci sono abbastanza abitanti liberi e postazioni disponibili; niente assegnazioni parziali, aggiornamento dei comandi dopo ogni cambio stato o selezione.
- **Stati UI:** i tre pulsanti di costruzione mantengono `CanAffordSelected/Five/Ten`, classe `primary build`, disabilitazione grigia e hover ocra senza spostamenti; i comandi personale hanno stati disabilitati espliciti. Lotti +5/+10 di costruzione conservano quote cumulative e tooltip.
- **Test:** sostituito il test del vecchio layout con una teoria a due risoluzioni (1040×740 e 1240×850), adattati i controlli sull’hover e sullo stato dei pulsanti, aggiunto test di assegnazione batch e rifiuto per limiti. **+2 casi**, **466 attesi**, non eseguiti in questo ambiente senza .NET 10.
- **Ambito:** solo ViewModel/XAML v14, test Desktop e documentazione; **v13, Core, economia, SQLite, Matite e salvataggi invariati**. [Report](docs/design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Test plan](docs/validation/G1_D27_RC11_TEST_PLAN.md) · [ADR 0036](docs/adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md).

## v60-E3b4-RC10 / G1-D26 — Fix compilazione dei test Matite (CANDIDATA, 03/11/2026)

- **Esito RC9 su Windows (utente):** build interrotta da **1 errore CS0246** in `V14SchoolSuppliesTests.cs:15` (`GameScenario` non risolto). Non sono stati eseguiti test xUnit per la RC9: non descrivere RC9 come 464/464 verdi.
- **Causa:** `GameScenario` è in `SimRisorse.Core.Content`, namespace non importato dal nuovo test Application.
- **Correzione unica al sorgente:** aggiunto `using SimRisorse.Core.Content;` in `tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs`; nessuna modifica a Core, Application produttiva, Desktop, XAML, SQLite, migrazioni, salvataggi o valori economici.
- **Evidenza:** nel log dell’utente gli altri otto progetti/assemblies indicati compilano; la soluzione globale non compila finché il riferimento del test non viene corretto. **464 test attesi in RC10**, non eseguiti in questo ambiente privo di .NET SDK 10.
- **Stato:** ultimo gate Windows positivo RC8 **456/456** confermato; RC10 **in attesa di validazione Windows e test grafici/manuali P0**. E4 rimane sospeso. [Diagnosi D26](docs/design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Piano D26](docs/validation/G1_D26_RC10_TEST_PLAN.md).


## v60-E3b4-RC9 / G1-D25 — Scuola con progressi e Matite (CANDIDATA, 03/11/2026)

- **RC8: 456 test superati su Windows**, confermati dall'utente il 03/11/2026. Nessuna dichiarazione di parità grafica già completata.
- Rimossi da entrambe le finestre i pulsanti visibili «Esci senza salvare». Conservate le modalità di chiusura tecnica dei test e il salvataggio ordinario.
- **Scuola v14** allineata al layout Esercito: abitanti, posti, coda, progressi individuali (barra e tempo primo allievo), qualificati e ricercatori attivi; costruzione resta soltanto in Edifici.
- Nuova risorsa **Matite** della sola v14; laboratorio manuale **1 Legno lavorato + 1 Carbone → 4 Matite**, nuova riga alfabetica nella tabella Risorse a 7 colonne con capienza condivisa (Municipio +1.000, Magazzino +500, Scuola +50). **2 Matite + 10 Cibo per iscrizione**; consumi e rimborsi atomici con vincoli di stoccaggio.
- Persistenza SQLite v3 sperimentale estesa con **campo opzionale `pencil_paid_students`**, leggendo ancora i salvataggi RC8 senza generare rimborsi Matite fittizi. Il clock preserva lo stock Matite e libera il diritto al rimborso quando termina la formazione.
- **8 test nuovi: 464 attesi**, non eseguiti qui senza SDK .NET 10. Modificati Core v14, SQLite v3, presentazione e XAML v13/v14 (solo per togliere il pulsante v13), test e documenti. **Catalogo v13 (16 risorse), simulazione v13 e SQLite v2 preservati.** Il confronto grafico P0 e il bilanciamento E4 sono ancora aperti.
- [Rapporto D25](docs/design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [ADR 0035](docs/adr/0035-scuola-formazione-matite-v14.md) · [Piano RC9](docs/validation/G1_D25_RC9_TEST_PLAN.md).

## v60-E3b4-RC8 / G1-D24 — Stati Edifici, pulsanti e Scuola (CANDIDATA, 03/11/2026)

- Corretta la GUI **v14** a seguito del feedback dell'utente: edifici bloccati per prerequisiti **grigi nell'elenco ma sempre selezionabili**; costi e motivazioni leggibili nel dettaglio. Le righe solo prive di materiali mantengono aspetto normale.
- Il pulsante principale **Costruisci +1** ora occupa tutta una riga e usa `IsEnabled=CanAffordSelected`, distinto da `IsVisible=CanBuildSelected`; stato disabilitato grigio, hover attivo ocra senza movimento.
- Seconda riga dedicata a **−1 lavoratore / +1 lavoratore / +5 / +10**; lotti disabilitati esplicitamente se non pagabili. Pannello dettagli v14 ampliato a 410 px per non troncare le etichette.
- Rimossa la costruzione duplicata dalla scheda Scuola: rimangono iscrizioni e operazioni ricercatori; costruzione Scuola disponibile soltanto nella scheda Edifici.
- **5 nuovi casi automatici** (3 test + hover in 2 viewport), aggiornato 1 test della Scuola: **456 test attesi, non eseguiti qui**. RC7 non è stata confermata verde; ultimo run comunicato: **RC6 450/451** per l'asserzione Avalonia errata, corretta in RC7.
- Modificati esclusivamente presentazione/XAML v14 e test Desktop, più documentazione. **V13, stili globali, Core, Application, SQLite e salvataggi invariati.** [Rapporto D24](docs/design/G1_D24_E3B4_RC8_STATI_EDIFICI_E_COMANDI.md) · [Piano D24](docs/validation/G1_D24_RC8_TEST_PLAN.md) · [ADR 0034](docs/adr/0034-v14-stati-edifici-e-layout-azioni.md).

Il changelog registra **cosa è stato introdotto**, non approva automaticamente gameplay, grafica o valori numerici. Date riferite al **3 novembre 2026**. Il file storico precedente, conservato integralmente in [`docs/history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md`](docs/history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md), contiene i dettagli e i checkpoint delle versioni anteriori; non è la fonte dello stato corrente.

Legenda: **Validata** = test Windows confermati dall'utente; **Candidata** = codice prodotto ma verifica pendente; **Respinta visivamente** = test automatici insufficienti per accettare la GUI; **Documentazione** = nessuna modifica funzionale.

## v60-E3b4-RC7 / G1-D23 — P0 allineamento test Avalonia 12 (CANDIDATA, 03/11/2026)

- **Esito reale della RC6 su Windows, fornito dall’utente:** **451 test eseguiti, 450 superati, 1 fallito** (`V14UiParityRegressionTests.UpgradeWithResearchButWithoutMaterialsRemainsVisibleAndDisabled`, riga 191). RC5 e RC6 sono dunque state esercitate insieme, **ma il gate complessivo non è verde**.
- **Causa verificata nel codice Avalonia 12.1.4:** il test leggeva `Button.IsEnabled` invece di `Button.IsEffectivelyEnabled`. `Button.IsEnabledCore` incorpora il risultato di `Command.CanExecute` nell'abilitazione effettiva, senza necessariamente modificare il valore `IsEnabled` impostato dall'utente. Prima dell'asserzione errata il test aveva già dimostrato che entrambi gli upgrade erano visibili, `CanUpgrade* == false`, `Command.CanExecute == false` e i costi riportavano «Mancano:».
- **Correzione circoscritta:** due asserzioni sull'abilitazione effettiva, più due controlli sull'identità dei comandi associati ai pulsanti. **Nessuna modifica** al ViewModel, simulazione, XAML, GUI v13, schemi DB, salvataggi o prezzi. Test count **invariato: 451**.
- **RC7 da validare** su Windows con `./validate.cmd`. Non dedurre un nuovo esito verde dai 450 test superati in RC6. Audit grafico v13/v14 ancora aperto.
- [Analisi causa D23](docs/design/G1_D23_E3B4_RC7_AVALONIA_TEST_ENABLED.md) · [Piano test D23](docs/validation/G1_D23_RC7_TEST_PLAN.md).

## v60-E3b4-RC6 / G1-D22 — P0 diagnostica produzione v14 (CANDIDATA, 03/11/2026)

- **RC5 non testata dall’utente:** a seguito dell’indicazione «non posso testare», prosegue lo sviluppo senza attribuire validazioni alla RC5. Ultima suite dichiarata verde: **RC4** (443 casi attesi; conteggio effettivo non comunicato).
- In Edifici v14 integrata la lettura diagnostica del **ProductionPlanner**: lavoratori produttivi/assegnati per filiera, input insufficienti, capacità di uscita, lotti e rimedi (Magazzino e, per Pane, Forno/Municipio; il Deposito alimentare non conserva Pane).
- Costi insufficienti per il prossimo acquisto e blocchi della produzione esistente sono mostrati insieme; addetti senza lavoro evidenziati. La misura di esecuzione è **aggregata Base/Grande/Avanzato**, non stimata per tier.
- **4 test Desktop** aggiunti: previsti **451** complessivi se discovery invariata. **Build, xUnit e audit Windows ancora pendenti**. Nessuna variazione a XAML, GUI v13, motore, catalogo, SQLite o regole economiche.
- [Rapporto D22](docs/design/G1_D22_E3B4_RC6_DIAGNOSTICA_PRODUZIONE.md) · [Piano RC6](docs/validation/G1_D22_RC6_TEST_PLAN.md).

## v60-E3b4-RC5 / G1-D21 — P0 preventivi +5/+10 e upgrade (CANDIDATA, 03/11/2026)

- **RC4: test dichiarati passati dall'utente** (443 erano attesi; non è stato fornito il conteggio effettivo né il log). Non considerare chiuso l'audit grafico Windows.
- Ripristinati nella GUI v14 preventivi cumulativi progressivi per **Costruisci +5/+10**, con mancanze quantità disponibili/richieste e tooltip senza cambiare l'elenco Risorse.
- Corretto l'errore per cui un upgrade sbloccato scompariva quando mancavano risorse: pulsante **visibile ma disabilitato**, prerequisiti tecnologici mancanti ⇒ pulsante assente e messaggio.
- La UI usa un unico calcolo quotazione per i pulsanti dei lotti +1/+5/+10; la simulazione, la GUI v13 e SQLite v2/v3 non sono state modificate.
- **4 test Desktop aggiunti**, nuovo totale **447 attesi, NON eseguiti qui**. Analisi statica e XML verificabili separatamente dalla prova Windows.
- [Rapporto D21](docs/design/G1_D21_E3B4_RC5_LOTTI_UPGRADE_GUI.md) · [Piano RC5](docs/validation/G1_D21_RC5_TEST_PLAN.md).

## v60-E3b4-RC4 / G1-D20 — P0 diagnostica Pane (CANDIDATA, 03/11/2026)

- Isolata una regressione concreta della RC3: la v14 nascondeva il fabbisogno Pane prima del primo residente avanzato e mostrava solo una spiegazione generica per gli stati successivi.
- Collegato lo **stesso `BreadSupplyProjector`** alle letture della sessione v13 e v14; adattatore read-only per i tier v14 e formatter condiviso fra le due GUI. Stati: primo arrivo, capacità nominale, input, spazio di stoccaggio, produzione prevista e riserva.
- Aggiunti **4 test** (3 Application, 1 Desktop): totale previsto **443**. **Non eseguiti nell’ambiente di confezionamento RC4**; la suite è stata successivamente confermata **passata dall’utente** il 3/11/2026 (numero effettivo non comunicato). Audit grafico ancora aperto.
- Nessuna modifica a XAML, clock, schemi SQLite, filiere economiche, logica salvataggi o GUI classica salvo delega alla stessa formattazione testuale.
- La parità RC3/RC4 non è certificata: resta indispensabile la matrice manuale completa.
- [Rapporto tecnico D20](docs/design/G1_D20_E3B4_RC4_DIAGNOSTICA_PANE.md) · [Piano di prova RC4](docs/validation/G1_D20_RC4_TEST_PLAN.md).

## G1-D19 — Riordino documentale per nuova chat (DOCUMENTAZIONE, 03/11/2026)

- Consolidati `README.md`, `CHANGELOG.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/NEW_CHAT_HANDOFF.md`, `docs/START_HERE.md`, `docs/CURRENT_STATE.md`, `docs/README.md`, `docs/AUTHORITY_REGISTRY.md`.
- Distinti **427 test RC1 confermati**, regressioni RC2 osservate e **439 test RC3 solo attesi**.
- Formalizzato il blocco delle nuove funzionalità finché la GUI v14 non raggiunge la parità con v13, con matrice di controllo e gate di uscita.
- Archiviati senza cancellarli changelog, handoff, roadmap, registro decisioni e indice documentale precedenti in `docs/history/operational/`.
- **Zero modifiche** a `.cs`, `.axaml`, progetti, script, salvataggi o asset. Nessun test .NET eseguito o dichiarato superato per questo riordino.

## v60-E3b4-RC3 — Correzione delle regressioni GUI (CANDIDATA)

- Dopo il feedback dell'utente, recupero della presentazione Risorse a sette colonne con `ResourceRow`, quantità/capacità, flussi e tempo al riempimento; ripristino dei dati demografici e dei comandi Esercito (arruola/annulla).
- Edifici bloccati: pulsanti Costruisci **non visibili**; sbloccati ma senza materiali: azioni **disabilitate**, con motivi del blocco. Ripristino pulsanti +1/+5/+10 e upgrade distinti; nessuna ComboBox di livello.
- Migliorata la protezione da salvataggi sperimentali incompatibili e la chiusura della finestra.
- Nove nuovi test rispetto ai 430 previsti per RC2, totale **439 attesi**; compilazione/xUnit e ispezione GUI Windows **pendenti**.
- Gap noto: diagnostica approfondita Pane e dettagli grafici/hover/scroll da confrontare integralmente con la v13; XAML v14 ancora duplicato.
- [Rapporto D18](docs/design/G1_D18_E3B4_RC3_RIPRISTINO_FUNZIONI_GUI.md) · [Piano RC3](docs/validation/V60_E3B4_RC3_PARITY_TEST_PLAN.md) · [ADR 0033](docs/adr/0033-preservazione-funzionale-gui-v13-v14.md).

## v60-E3b4-RC2 — Riallestimento classico incompleto (RESPINTA VISIVAMENTE)

- Introdotti menu laterale e livelli produttivi come righe di edificio distinte, senza ComboBox nel dettaglio.
- **Regressioni segnalate dall'utente:** pagina Risorse semplificata, comandi Costruisci mostrati su edifici ancora bloccati, mancanza di altre funzioni già presenti nella vecchia GUI; Esercito non completo.
- +3 test su RC1, **430 attesi**. Il riordino non costituisce accettazione della RC2.
- [Rapporto D17](docs/design/G1_D17_E3B4_RC2_RIALLESTIMENTO_GUI_CLASSICA.md) · [ADR 0032](docs/adr/0032-gui-v14-identica-navigazione-classica-tier-come-edifici.md).

## v60-E3b4-RC1 — Anteprima Avalonia separata (VALIDATA NEI TEST, GUI NON APPROVATA)

- Avvio esplicito `--experimental-v14`, SQLite v3 separato e sezione sperimentale per Scuola, tecnologia, produttori a livelli, civili.
- 10 test Desktop introdotti, **427 test passati su Windows secondo l'utente**.
- UI sperimentale iniziale giudicata non soddisfacente; l'esito verde xUnit **non equivale** a correttezza grafica o parità funzionale.
- [Rapporto D16](docs/design/G1_D16_E3B4_ANTEPRIMA_DESKTOP_V14.md) · [ADR 0031](docs/adr/0031-e3b4-gui-v14-opt-in.md).

## v60-E3b3-RC5 — Sessione v14 e replay (VALIDATA)

- Aggiunta costruzione civile (Case, Magazzino, Caserma) e confronto di due sessioni equivalenti con salvataggio/ripristino SQLite nel mezzo.
- **417/417 test confermati** dall'utente; valori di economia ancora sperimentali.
- [Rapporto D15](docs/design/G1_D15_E3B3E_REPLAY_CIVILI_V14.md).

## v60-E3b3-RC4 — Livelli, costruzioni e upgrade (VALIDATA)

- Comandi v14 per costruzione diretta Base/Grande/Avanzato, upgrade, assegnazione/rilascio lavoratori, prezzi progressivi e verifiche su risorse e tecnologie.
- **408/408 test confermati** dall'utente. [Rapporto D14](docs/design/G1_D14_E3B3D_COSTRUZIONE_UPGRADE_TIER_V14.md).

## v60-E3b3-RC3 — Fixture orologio (VALIDATA)

- Corretta popolazione iniziale dei test del clock; vincolo Core contro duplicazioni di lavoratori/ricercatori/militari mantenuto.
- **396/396 test confermati** dall'utente. [Rapporto](docs/validation/V60_E3B3_RC3_FIX_CLOCK_FIXTURE.md).

## v60-E3b3-RC2 — Clock unico v14 (SOSTITUITA DA RC3)

- Avanzamento coerente di economia, crescita, addestramento e ricerca, ma **7 test fallivano** per dati della fixture: 389/396 superati su Windows. Corretti nella RC3.

## v60-E3b3-RC1 — Comandi Scuola, ricercatori e tecnologie (VALIDATA)

- Sessione v14 isolata, creazione Scuola, formazione, assegnazione ricercatori e acquisto volontario tecnologie; **387/387 test confermati**.

## v60-E3b2-RC3 — SQLite v3 con controllo studenti (VALIDATA)

- Controllo di integrità della coda di formazione in SQLite v3; **380/380 test confermati**.

## v60-E3b2-RC2 — Fix e pulizia directory principale (VALIDATA)

- Corretti confronti ordine dati SQLite nei test e test della posizione formativa eliminata; archiviati 25 file storici della root in `docs/history/`. **377 test superati** dopo la correzione.

## E1 / E2 / E3 / E3b/1 — Fondamenta v14 (IMPLEMENTATE, POI VALIDATE INDIRETTAMENTE)

- E1: prezzi progressivi per famiglia e saldo di ricerca liberamente accumulabile.
- E2: Base/Grande/Avanzato, lavoratori e capacità locali, upgrade separati.
- E3: Scuola, formazione e otto tecnologie produttive sperimentali.
- E3b/1: snapshot v14, conversione v13→v14 in memoria e controlli di consistenza.
- Modelli confluiti nel ramo della sessione v14 che ha superato gate successivi. **Non** significa che costi, tempi o coefficienti siano approvati.

## v59-RC1 e precedenti — Campagna storica (VALIDATA)

- Ruleset `civil-growth-13`, Municipio, produzione e consumi, Magazzino/Pane condivisi, abitazioni, filiera Pane, Ferro/Oro/Forgia/Zecca, Caserma e addestramento individuale.
- Ultima campagna normale conservata nel ramo sperimentale v60. Cronologia e risultati dettagliati nello [storico integrale](docs/history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md).

## Stato successivo

**Prima:** RC5 `./validate.cmd` e confronto visivo/funzionale con v13. **Poi:** eliminare le disparità rimaste e condividere componenti GUI senza cambiare il layout originale. **Solo dopo:** bilanciamento e approvazione quantitativi E4, migrazione sicura, nuova campagna; quindi navale M5.4 e altre milestone. [Roadmap dettagliata](docs/ROADMAP.md).
