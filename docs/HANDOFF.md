# Handoff operativo — RC38 / E4a-D01-F1

**Usare l'archivio RC38 integrale.** Il test cumulativo Windows RC37 ha dato **540/541 PASS**. Unico fallimento in `V14SchoolResearchEndToEndTests` per diversa posizione di record `OtherBuildings` equivalenti dopo SQLite: il test RC38 normalizza le collezioni con identità, senza modificare simulazione o database. **541 test attesi su RC38; non ancora validata**. Ultima versione interamente confermata RC30 (511). Il benchmark E4A-06/07 non è stato ancora eseguito: non attribuire misure o tempi. Eseguire prima `./validate.cmd`, poi due run distinte del benchmark se verde.

**Vincoli:** v14 predefinita, v13 legacy isolata; nessun coefficiente E4b modificato; salvataggi reali protetti. [Rapporto](design/E4A_D01_RC38_FIX_ORDERING_SQLITE.md) · [Piano test](validation/E4A_D01_RC38_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff operativo — RC37 / E4a-D01

Usare **l'archivio RC37 integrale**. Ultimo gate Windows confermato RC30 (511 test); RC31–RC37 devono essere validate cumulativamente (**541 test attesi**). **Non dichiarare alcun benchmark misurato finché la CLI non viene eseguita**. Il runner `--benchmark-e4a-school` esercita scenari **controllati** E4A-06/07 da stock aggiunti (+600 planks, +400 stone, +10 coal), non prova l'accessibilità della Scuola in una partita naturale. Output CSV/JSON in `.local-cache`, nessun overwrite di file di misura.

**Invarianti:** v14 predefinita, v13 legacy e slot isolati; 2 Matite per allievo, ricerca cumulabile, livelli Base/Grande/Avanzato distinti. Motore, GUI, DB e coefficienti invariati in RC37. Proseguire con D02 benchmark naturali, D03 Carbone/stoccaggio, D04 persistenza/report, poi E4b **solo su approvazione**. P1-D50 resta in attesa del gate cumulativo.

[Rapporto](design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) · [Validazione](validation/E4A_D01_RC37_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff operativo — RC36 / P1-D50

**Baseline Windows confermata RC30 (511 test).** RC36 contiene cumulativamente RC31–RC35 e aggiunge due test di persistenza per studenti pre-Matite e rifiuto di ledger impossibile: **538 test attesi, NON eseguiti su Windows**. Nessuna modifica ai sorgenti applicativi o schema salvataggi. La v14 è la GUI predefinita; la v13 resta accessibile con opzione legacy e database distinto.

**Gate:** eseguire `./validate.cmd` quando possibile; controllare flusso Scuola→Tecnologie e Salva/Carica su copie. P1 rimane aperta fino al gate. Preparato [protocollo E4a](design/E4A_PROTOCOLLO_BENCHMARK.md) con [10 scenari](../benchmarks/e4a/scenarios.csv); non sono benchmark eseguiti né approvazioni dei coefficienti. Segue E4a → E4b su approvazione → E4c → M5.4–M7.

[Rapporto D50](design/P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md) · [Piano test](validation/P1_D50_RC36_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff prioritario — RC35 / P1-D49

Ripartire dall'archivio integrale RC35. Ultima conferma automatica RC30 (511 test); RC31–RC35 non ancora validate su Windows (**536 attesi**). P1-D49 aggiunge tre test di integrazione reali nella suite Infrastructure: Matite→formazione→ricercatore→punti→Lavorazioni I→Segheria Grande, save/load SQLite, pausa e azioni atomiche. Nessuna modifica al codice del gioco/SQLite, né al bilanciamento. **Prossimo P1-D50:** gate scuola/tecnologie, registro decisioni confermate/candidate, predisposizione benchmark E4a. Evitare ritorni alla GUI classica salvo `--legacy-v13`, che deve restare recuperabile senza scrivere sullo slot v14.

[Roadmap](ROADMAP.md) · [Rapporto D49](design/P1_D49_RC35_INTEGRAZIONE_PROGRESSIVA.md) · [Validazione RC35](validation/P1_D49_RC35_TEST_PLAN.md)

---

# Handoff prioritario — RC34 / P1-D48

Ripartire da **RC34 integrale**. Ultimo checkpoint testato dall'utente: RC30 (511); RC31, RC32, RC33 e RC34 non ancora validate su Windows. RC34 contiene **533 test attesi**. La v14 rimane predefinita, v13 legacy con DB separato.

P1-D48: il catalogo ricerca viene confrontato con le regole tier reali all'avvio/ripristino; ogni tecnologia acquistabile deve sbloccare famiglie realmente implementate. Il dettaglio della tecnologia mostra Grande/Avanzato e relativi edifici; Logistica/Navigazione rimangono differite. Sono stati aggiunti 9 casi xUnit; nessuna modifica a prezzi, ricette, progressi, SQLite o GUI v13.

**Prossimo:** P1-D49 scenari end-to-end (Matite → studente → ricercatore → tecnologia → Grande/Avanzato, salvataggi), P1-D50 gate, poi E4a/E4b/E4c e M5.4–M7. Richiesta consegna ZIP integrale a ogni avanzamento.

[Rapporto](design/P1_D48_RC34_ALBERO_TECNOLOGICO.md) · [Piano test](validation/P1_D48_RC34_TEST_PLAN.md) · [Roadmap](ROADMAP.md)

---

# Handoff prioritario — RC33 / P1-D47

Usare **RC33 integrale**. Ultima release testata dall'utente RC30 (511 test previsti); RC31, RC32 e RC33 **non validate su Windows**, RC33 conta 524 test attesi. La GUI v14 resta predefinita, v13 disponibile come modalità legacy isolata.

P1-D47: laboratorio Matite con check di fattibilità unico e di sola lettura fra sessione e GUI; suggerimenti per mancanza di entrambi gli input e dello spazio per lotti +1/+5/+10. Test per atomicità, rimborsi legacy/misti e coerenza del bottone. Non cambiare ricetta, tempo formazione, min 2 Matite/studente, stoccaggio, SQLite o bilanciamento fino a E4a/b.

**Prossimo:** P1-D48 inventario tecnologie, prerequisiti, acquisti ed effetti reali, poi D49 integrazione, D50 gate. Sempre ZIP integrale, roadmap, changelog, handoff, test. [Rapporto](design/P1_D47_RC33_FILIERA_MATITE.md) · [Piano test](validation/P1_D47_RC33_TEST_PLAN.md).

---

# Handoff prioritario — RC32 / P1-D46

Ripartire da **RC32 integrale**. L'utente ha confermato i test RC30 (511 previsti), ma **RC31 e RC32 non sono state ancora validate su Windows**. RC31 rende la v14 predefinita, mantenendo v13 via `--legacy-v13` e database separati. RC32 aggiunge progressi individuali visibili per fino a 6 allievi (ulteriori conteggiati), con 3 test; **518 test cumulativi attesi**.

**Invarianti:** UI v14 approvata senza redesign; 7 colonne Risorse; edificio Base/Grande/Avanzato separati; Matite 2/studente e costo Cibo; reclutamento e ricercatori senza doppio conteggio; salvataggi v13 protetti. Non modificare parametri E4 senza approvazione. Prossima P1-D47 audit filiera Matite.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [D46](design/P1_D46_RC32_CICLO_FORMAZIONE_SCUOLA.md) · [Validazione](validation/P1_D46_RC32_TEST_PLAN.md)

---

# SimRisorse — RC31 / G1-D45: v14 predefinita (candidata)

**Aggiornamento 04/11/2026:** la **RC30** ha superato i test secondo la conferma dell'utente (**511 test previsti**). La GUI v14 è stata dichiarata soddisfacente e diventa l'interfaccia ordinaria; lo storico dossier di **44 prove visive non acquisite** resta disponibile ma non è più prerequisito per avviare P1. **RC31** aggiunge quattro test di routing (515 attesi) e deve ancora essere compilata/testata su Windows.

Avvio: `.\run-game.cmd` → v14, sullo **stesso** slot SQLite v14 già usato (`%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`). `.\run-game.cmd --legacy-v13` → v13, DB storico separato. `--experimental-v14` funziona ancora come alias. `--scenario ...` e `--smoke-test` rimangono legacy per compatibilità. Nessuna migrazione automatica né modifica al bilanciamento.

**Prossima milestone:** P1-D46 audit ciclo Scuola; D47 Matite; D48 tecnologie; D49 integrazione; D50 gate. Quindi E4a → E4b → E4c → M5.4–M7. [ADR 0042](adr/0042-promozione-v14-e-conservazione-slot.md) · [Rapporto D45](design/G1_D45_RC31_PROMOZIONE_V14.md) · [Piano RC31](validation/G1_D45_RC31_TEST_PLAN.md).

---

# Handoff operativo — G1-D44 / RC30

**Riprendere da RC30 integrale.** Ultima baseline testata dall'utente: **RC27 507/507**. RC28 ha introdotto il focus da tastiera, RC29 gli strumenti D43, RC30 ne rafforza i controlli: **511 test xUnit attesi, non ancora eseguiti** su Windows. Aggiunto `scripts/test-p0-ui-evidence.ps1` come test **separato** dai 511. Prove visive reali 0/44: **non dichiarare P0 chiuso né promuovere P1/E4**.

RC30 cambia solo `scripts/p0-ui-evidence.ps1`, aggiunge il suo test standalone e documentazione; v13, v14, Core, SQLite e test .NET restano invariati. Lo strumento valida il formato/dimensione dei PNG, blocca i 44 NA da esito completo e offre `-Action Verify` (0 = solo evidenze formalmente complete, 2 = incomplete). Non sostituisce le 44 prove reali e l'approvazione visiva.

**Prossimi passi:** quando possibile Windows `validate.cmd` (511 attesi) e `powershell.exe -NoProfile -File .\scripts\test-p0-ui-evidence.ps1`, poi 44 prove G1-D43. In D44 correggere solo difetti **osservati e documentati**; in D45 gate manuale. Non cambiare bilanciamento E4 né alterare la GUI classica.

[Roadmap](ROADMAP.md) · [Stato](CURRENT_STATE.md) · [Rapporto D44](design/G1_D44_RC30_RAFFERZAMENTO_EVIDENZE_P0.md) · [Piano test](validation/G1_D44_RC30_TEST_PLAN.md).

---

# Handoff — G1-D43 / RC29

**Archiviare/conservare RC29 integrale.** Ultimo checkpoint Windows confermato **RC27 (507 test)**; RC28 ha 4 nuove esecuzioni Desktop e **511 test attesi**, non ancora eseguiti. RC29 non modifica il gioco o la suite .NET: aggiunge `scripts/p0-ui-evidence.ps1` e la documentazione per 44 scenari. **Non dichiarare RC28/RC29 verdi o la GUI P0 approvata:** nessuna prova visiva delle 22 voci è stata fatta in questa consegna.

Quando Windows sarà disponibile: `./validate.cmd` e `./scripts/p0-ui-evidence.ps1 -Action Init`; compilare il CSV su copie delle partite; screenshot con `Alt+Stamp` + `-Action Capture`, o copie PNG con lo stesso nome; infine `-Action Report` per confronto affiancato e lacune. **Il report HTML non firma da solo il gate**. D44 solo correzioni verificate; D45 gate con conferma esplicita. Seguono P1, E4a, E4b, E4c, M5.4–M7.

[Roadmap](ROADMAP.md) · [Rapporto statico D43](design/G1_D43_RC29_AUDIT_VISIVO.md) · [Piano D43](validation/G1_D43_RC29_PIANO_EVIDENZE.md).

---

# Handoff — RC28 / G1-D42

**Riprendere dall'archivio integrale RC28.** L'utente ha confermato che **RC27 ha superato 507 test**. RC28 aggiunge uno stile di focus **solo alla finestra v14** e quattro esecuzioni di test Desktop headless, quindi **511 test attesi ma non eseguiti**. Nessuna modifica alla GUI classica, al motore, ai database o ai coefficienti economici.

**Prossime operazioni:** eseguire `.\validate.cmd` sulla RC28 e raccogliere evidenze focus/tastiera a 1040×740 e 1240×850; avviare G1-D43 sulle **22 voci P0** soltanto quando disponibili test e prove Windows. La matrice non è ancora approvata; non dichiarare la GUI certificata. D44 corregge difetti dimostrati, D45 gate P0; poi P1-D46…D50, E4a/E4b/E4c e M5.4–M7.

**Vincoli:** preservare GUI v13, sette colonne Risorse, selezione ocra, edifici grigi ma selezionabili, righe Costruisci/Lavoratore, Scuola/Matite, salvataggi isolati. Consegnare ZIP integrale e aggiornare i documenti di handoff.

[Rapporto D42](design/G1_D42_RC28_FOCUS_TASTIERA.md) · [Piano test](validation/G1_D42_RC28_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff operativo post-RC27 / pianificazione D42

**RC27 confermata su Windows con 507 test superati (comunicazione dell'utente).** G1-D42 è documentazione, nessuna RC successiva né sorgenti/test modificati. Le 22 prove visive P0 non sono ancora certificate. Prossima implementazione: D42 audit focus e tastiera, con correzione solo di anomalie riproducibili; poi D43 prove 1040×740 e 1240×850, D44 fix, D45 gate. Successivamente P1-D46…D50, E4a, E4b, E4c, M5.4–M7. Leggere la [roadmap canonica](ROADMAP.md), il [runbook](validation/G1_D42_P0_RUNBOOK.md) e l'[ADR 0040](adr/0040-pianificazione-post-rc27-e-gate-sequenziali.md). Non inventare test passati, screenshot o decisioni economiche.

---

# Handoff RC27 / G1-D41

**Usare l’archivio integrale RC27**, non RC26. **RC26: 504 test superati dichiarati dall’utente.** RC27 corregge solo la v14 nel dettaglio Edifici: entrambi gli ScrollViewer tornano all'inizio quando cambia la selezione; la lettura non viene interrotta da aggiornamenti della stessa riga. 3 nuovi test Desktop, **507 attesi**, non eseguiti qui per SDK .NET 10 assente. Nessuna modifica alle logiche di gioco, GUI v13, SQLite o salvataggi. **P0 visivo ancora aperto**: 22 voci alle due risoluzioni; D42 per focus/tastiera e anomalie residue. Solo dopo P0 passare a P1, E4a/b/c e M5.4–M7.

[Rapporto](design/G1_D41_RC27_AUDIT_SCORRIMENTO_DETTAGLI.md) · [Test RC27](validation/G1_D41_RC27_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Matrice](validation/G1_D39_P0_UI_MATRIX.md).

---

# Handoff RC26 / G1-D40

**Usare lo ZIP integrale RC26**, non i vecchi pacchetti. **RC25 confermata verde dall’utente (501/501)**. RC26 aggiunge 3 test Desktop (504 attesi), correggendo solo il testo v14 della crescita per recuperare ventesimi di progresso e ETA del prossimo abitante accelerato (×1,15), e protegge il tempo di riempimento UI-05 con test sul saldo netto. **Nessuna modifica a v13, Core, SQLite, XAML o coefficienti E4.** Prossima azione: `./validate.cmd` su RC26 quando possibile; confronto visivo UI-05/UI-14 e altre voci della [matrice P0](validation/G1_D39_P0_UI_MATRIX.md) alle risoluzioni 1040×740 e 1240×850. **P0 aperto**, poi P1, E4a/b/c e M5.4–M7.

[Rapporto D40](design/G1_D40_RC26_FLUSSI_POPOLAZIONE.md) · [Piano RC26](validation/G1_D40_RC26_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff RC25 / G1-D39-F1

**Partire da RC25, non da RC24.** RC20 è l'ultima suite Windows confermata (490/490). RC24 non ha raggiunto xUnit a causa di due CS1061 su `MouseMove` nel nuovo test Avalonia; RC25 aggiunge l'import corretto, nessun cambiamento all'applicazione. **501 test attesi**, verifica su Windows pendente.

**Passo successivo:** `./validate.cmd` su RC25, poi [22 prove P0](validation/G1_D39_P0_UI_MATRIX.md) con screenshot a 1040×740 e 1240×850; D40 audit UI-05/UI-14 e, solo dopo chiusura P0, P1 ed E4. [Stato](CURRENT_STATE.md) · [Report F1](design/G1_D39_RC25_FIX_CS1061_HEADLESS.md) · [Test plan RC25](validation/G1_D39_RC25_TEST_PLAN.md).

---

# Handoff RC24 / G1-D39

**Usare lo ZIP integrale RC24.** RC20 è l'ultimo checkpoint Windows confermato (490 test). **RC21–RC24 non ancora validati**: 493, 496, 499, **501 attesi** rispettivamente; eseguire soltanto `./validate.cmd` su RC24 per la suite cumulativa quando possibile. Durante la conferma «Nuova partita?» i comandi Scuola erano disabilitati ma i suggerimenti di fabbricazione Matite, iscrizione e annullamento non menzionavano il blocco: D39 li allinea, +2 test. Nessuna nuova regola o cambiamento XAML/SQLite/v13.

**Priorità:** [matrice P0 con 22 gate](validation/G1_D39_P0_UI_MATRIX.md), screenshot/risultati reali a 1040×740 e 1240×850, quindi G1-D40 flussi e crescita. Nessun gate grafico certificato e nessun E4 avviato. [Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Report](design/G1_D39_RC24_AUDIT_SCUOLA_E_GATE_P0.md) · [Test plan](validation/G1_D39_RC24_TEST_PLAN.md).

---

# Handoff RC23 / G1-D38

**Baseline confermata:** RC20 (490 test). **RC21:** 493 attesi, **RC22:** 496 attesi, **RC23:** 499 attesi — tutti da verificare cumulativamente su Windows, nessuno dei tre dichiarato verde. D38 non modifica applicazione: aggiunge tre test Desktop per menu laterale bidirezionale, 5 schede, selezioni risorsa/edificio/tecnologia conservate al tick e in Save/Load. Area UI-01/UI-19, P0 visivo aperto. Preservare v13, database, livelli distinti, tabella Risorse 7 colonne e i comandi Edifici/Scuola/Esercito. Non iniziare E4.

[Stato](CURRENT_STATE.md) · [Rapporto D38](design/G1_D38_RC23_AUDIT_NAVIGAZIONE_SELEZIONE.md) · [Piano](validation/G1_D38_RC23_TEST_PLAN.md).

---

# Aggiornamento handoff — RC22 / G1-D37

**Verde noto:** RC20 (490 test). **RC21 (493) e RC22 (496 attesi) da verificare su Windows**; la versione cumulativa da estrarre è RC22. Audit P0 delle 22 voci aperto, nessun E4. La diagnosi Edifici v14 è scrollabile con limite di 104 px senza comprimere i comandi; aggiunti due test viewport e un test del progresso militare. **Conservare v13 e salvataggi SQLite separati.** [Report D37](design/G1_D37_RC22_SCORRIMENTO_DETTAGLI_ESERCITO.md) · [Piano test](validation/G1_D37_RC22_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff aggiornato — RC21 / G1-D36

**RC20 validata:** 490 test attesi dichiarati superati dall'utente. **RC21 candidata:** 493 test attesi (+3 Desktop), da compilare e testare su Windows. Include integralmente RC17–RC20 ora coperti dal gate RC20, senza altre modifiche a logica, salvataggi o produzione.

**D36:** `V14PreviewWindow.axaml` collega `V14UpgradeNext` / `V14UpgradeAdvanced` rispettivamente a `CanUpgradeNext` / `CanUpgradeDirectlyToAdvanced` tramite `IsEnabled` esplicito e applica lo stile `primary build`. Senza materiali i bottoni rimangono visibili con i preventivi, ma grigi e senza hover attivo; se pagabili sono verdi, hover ocra fisso. Tre test in `V14BuildingUpgradeAuditTests.cs` (due viewport + upgrade con costo preciso). Non cambia regole, disponibilità logica, GUI v13 o SQLite.

**Prossima G1-D37:** completare audit Edifici/Esercito e l'esame visivo delle 22 voci, 1040×740 e 1240×850; P0 ancora aperto, E4 non iniziato. **Consegna:** sempre ZIP completo, change log, roadmap, handoff, test; distinguere suite Windows da verifiche statiche.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Piano](validation/G1_D36_RC21_TEST_PLAN.md) · [Rapporto](design/G1_D36_RC21_AUDIT_EDIFICI_UPGRADE.md).

---

# Handoff aggiornato — RC20 / G1-D35

**Ultimo test Windows dichiarato superato:** RC16 (476 test). **Versioni non testate:** RC17 (481), RC18 (484), RC19 (487), **RC20 (490 attesi)**. RC20 include tutte le versioni intermedie; testare direttamente lo ZIP integrale RC20.

**RC20 contiene solo test e documentazione.** Nuovo `tests/SimRisorse.Desktop.Tests/V14ResourcesAuditTests.cs` (3 casi) per verificare le sette colonne della GUI v14, gutter scrollbar, sei colonne numeriche, selezione ocra, tooltip Capacità, progressione tick, ordinamento e 17 risorse. La GUI v13, la GUI v14, il motore e gli schemi SQLite sono invariati. Nessuna correzione grafica è stata inventata senza una regressione dimostrata.

**Prossimo G1-D36:** audit Edifici. Non perdere le due righe Costruisci/Lavoratore, edifici bloccati grigi ma selezionabili, costi cumulativi e stati `IsEffectivelyEnabled`. I test RC17 del checkpoint sono un gate delicato ancora da validare. P0 resta aperto finché Windows e screenshot non confermano le 22 voci; E4 e navale sospesi.

[Rapporto](design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md) · [Piano RC20](validation/G1_D35_RC20_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff aggiornato — RC19 / G1-D34

**RC16 è l'ultimo gate confermato:** 476 test Windows. **RC17** (481), **RC18** (484) non testate. **RC19** include RC17+RC18 e introduce 3 test, **487 totali attesi**, senza compilazione/test Windows in questo ambiente.

**Patch RC19:** `src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.Parity.cs` allinea i suggerimenti Esercito al blocco dei comandi durante la conferma «Nuova partita?» e mostra deficit numerici secondo lotto. Corretto «1 recluta». Nuovo `tests/SimRisorse.Desktop.Tests/V14MilitaryUiParityTests.cs` (3 casi). Invariati GUI v13, XAML, Core, Application, Infrastructure, SQLite, coefficienti, salvataggi. **RC17 è una modifica importante della protezione del checkpoint, ancora da convalidare su copia.**

**Prossimo lavoro:** eseguire quando possibile ` .\validate.cmd` su RC19 (487 previsti); poi audit Risorse/Edifici e delle 22 voci a due risoluzioni. Non dichiarare P0 approvato con soli test automatici. E4 e navale restano sospesi.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Rapporto D34](design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Piano RC19](validation/G1_D34_RC19_TEST_PLAN.md).

---

# Handoff aggiornato — RC18 / G1-D33

**RC16:** 476 test Windows confermati dall'utente. **RC17:** 481 attesi, utente temporaneamente impossibilitato a testarli. **RC18:** 484 attesi, non compilati/eseguiti qui. Partire **dallo ZIP integrale RC18**, che contiene anche RC17; non dichiarare verde fino al test Windows.

**RC18:** migliorati i messaggi di assegnazione dei ricercatori nei lotti, con posti di ricerca insufficienti anche se ne resta almeno uno; 3 test nuovi. Invariati algoritmo economico, Core, SQLite, v13, XAML v14. **RC17:** continua a proteggere checkpoint/autosalvataggio in nuova partita; richiede la validazione su Windows.

Preservare tabella Risorse a sette colonne, lista Edifici Base/Grande/Avanzato, pulsanti Costruisci +1/+5/+10 e Lavoratore −1/+1/+5/+10, Esercito, Scuola/Matite, ricerca a due decimali, GUI retro v13. Gate P0 non chiuso. Prossima priorità: audit Risorse, Edifici ed Esercito e screenshot a 1040×740 / 1240×850. Niente E4, navale né cambi di DB.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Matrice](validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Rapporto D33](design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Piano test D33](validation/G1_D33_RC18_TEST_PLAN.md).

---

# Handoff aggiornato — RC17 / G1-D32

**Ultimo gate Windows confermato:** RC16 con **476 test passati**. **Candidata RC17:** **481 test attesi** (+5 Desktop), test .NET non eseguiti in questo ambiente. D32 corregge la collisione Salva/Carica con la conferma «Nuova partita» e impedisce che il timer/chiusura sovrascriva il vecchio checkpoint v14 dopo Avvia nuova prima di un **Salva v14** manuale. Chiudere durante la conferma la annulla e salva la vecchia colonia. I salvataggi v13 e v14 restano separati. **Gate P0 visivo aperto, E4 rimandato.**

**Leggere per la ripresa:** [CURRENT_STATE](CURRENT_STATE.md), [ROADMAP](ROADMAP.md), [audit 22 voci](validation/G1_D29_AUDIT_GUI_V13_V14.md), [rapporto G1-D32](design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md), [piano RC17](validation/G1_D32_RC17_TEST_PLAN.md), [ADR 0039](adr/0039-protezione-checkpoint-e-autosalvataggio-v14.md).

---

# Handoff tecnico completo — SimRisorse RC16 / G1-D31

**Baseline validata:** RC15 (473 test attesi, dichiarati superati dall’utente). **Candidata RC16:** +3 test Desktop, 476 attesi e non eseguiti nell’ambiente di produzione. La RC16 blocca le azioni che modificano la colonia durante «Nuova partita?», fino a Conferma o Annulla; mantiene visibilità e dettagli della GUI e preserva l’intero modello/salvataggi. Patch D30: capienza Matite nel dettaglio Edifici (Municipio/Magazzino/Scuola) e disabilitazione Pausa/Riprendi durante conferma nuova partita, come v13. Il Core v14 introduce soltanto una costante nominata per `50` posti Matite/Scuola senza modifica di formula; GUI v13, XAML, SQLite e salvataggi invariati. **Gate grafico P0 aperto; E4 sospeso.**

## Ordine di lettura

1. [Stato corrente](CURRENT_STATE.md) — evidenze reali e gate.
2. [Roadmap ufficiale in sei fasi](ROADMAP.md) — sequenza, decisioni, stop rule e risultati attesi.
3. [Audit UI-01…UI-22](validation/G1_D29_AUDIT_GUI_V13_V14.md) e [matrice originale](validation/G1_D19_MATRICE_PARITA_GUI.md).
4. [Piano validazione RC16](validation/G1_D31_RC16_TEST_PLAN.md), [rapporto RC16](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md), [Piano validazione RC15](validation/G1_D30_RC15_TEST_PLAN.md), [report D30](design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md), [piano storico RC14](validation/G1_D29_RC14_TEST_PLAN.md), [diagnosi CS0136](design/G1_D29_RC14_FIX_CS0136.md), [piano RC13](validation/G1_D29_RC13_TEST_PLAN.md), [rapporto D29](design/G1_D29_RC13_AUDIT_E_TOOLTIP_MATITE.md), [ADR 0038](adr/0038-roadmap-gate-gui-e-audit-matite.md).
5. [Registro delle decisioni](AUTHORITY_REGISTRY.md), [architettura](ARCHITECTURE.md) e [contratto GUI](PIXEL_ART_UI.md).

## Vincoli funzionali da non perdere

- **v13 immutata:** GUI `MainWindow`, 16 risorse, campagna `civil-growth-13`, SQLite v2 in `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`.
- **v14 opt-in:** `--experimental-v14`, SQLite v3 in `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`; nessuna migrazione o sovrascrittura automatica.
- **Risorse:** sette colonne (Risorsa, Quantità, Capacità, Produzione, Consumo, Saldo, Si riempie tra), quantità tabella arrotondate, dati precisi a tooltip, capacità reale e fonte, ordine alfabetico, scrollbar e selezione ocra; Pane uniformato.
- **Edifici:** Base/Grande/Avanzato **distinti**, no ComboBox livello. Prerequisiti mancanti ⇒ riga grigia ma cliccabile, requisiti leggibili, pulsanti costruzione nascosti. Risorse mancanti ⇒ pulsanti grigi/disabilitati, costo e causa visibili. Riga `Costruisci | +1 | +5 | +10`; riga `Lavoratore (−1) | +1 | +5 | +10`. Lotti cumulativi, upgrade separati, hover ocra immobile.
- **Scuola:** costruibile solo da Edifici, nella pagina Scuola ci sono progressi come Esercito, iscrizione/annullamento, assegnazione ricercatori e laboratorio Matite. Ogni iscritto spende **almeno 2 Matite**. Ricetta candidata: 1 Legno lavorato + 1 Carbone → 4 Matite. Niente «Esci senza salvare».
- **Albero:** saldo e prezzi visualizzati a due decimali; acquisto su comando esplicito, confronto esatto in millipunti, disabilitato se risorse/prerequisiti non soddisfatti. Le tecnologie senza effetto non sono acquistabili.
- **Esercito:** non perdere Caserme, Spade, reclute, tempi individuali, arruolamento/annullamento. Residenti non duplicati con lavoratori, ricercatori, studenti o soldati.
- **Economia:** prezzi +15%, livelli ×1/2,6/6,5, +50/+150/+500 di storage, tempo formazione 120 s, 0,1 punti/s ricercatore sono **candidati**, non valori approvati; livelli 1/3/10 personale sono requisito confermato.

## Modifica RC13 e correzione compilazione RC14

`src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.Parity.cs`: il tooltip Matite non usa più l'elenco generico dei bonus potenziali. Calcola la quota degli edifici realmente posseduti dalla definizione `StoragePerBuilding` e usa la capacità effettiva Core per la parte Scuola; indica quantità, spazio libero e totali. Due casi aggiunti a `tests/SimRisorse.Desktop.Tests/V14UiParityRegressionTests.cs`. **RC14:** rinominati esclusivamente `owned`→`ownedBuilding` e `free`→`freePencilSpace` all’interno del ramo Matite per risolvere il conflitto CS0136; invariato il risultato della formula.

**Non modificati:** `src/SimRisorse.Desktop/Views/*.axaml`, v13, Core, Application, Infrastructure, catalogo, schema e migrazioni SQLite.

## Comandi e prossimo lavoro

Windows 11 + .NET SDK 10 (`global.json`: 10.0.200, rollForward latestFeature):

```powershell
.\validate.cmd
.\run-game.cmd
.\run-game.cmd --experimental-v14
```

Eseguire **476 test previsti RC16** e riportare log/numero reale (RC15 già validata); RC13 non è arrivata ai test perché non compilava. Poi D30+: confronto visivo per ciascuna delle 22 voci con screenshot alle due risoluzioni; correggere prima gli scostamenti P0. Non iniziare E4 o navale senza gate.

**Consegna obbligatoria a ogni incremento:** ZIP integrale, test e stato esplicito, changelog, roadmap, handoff, ADR se cambiano decisioni. Conservare snapshot originali e dati personali, senza incorporarli nello ZIP.

Per il contesto dettagliato precedente, la versione G1-D28 di questo documento è in [archivio storico](history/operational/HANDOFF_G1_D28_ARCHIVE.md).

[Report RC15](design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md) · [Piano RC15](validation/G1_D30_RC15_TEST_PLAN.md).

**RC16 / D31:** guardia `CanMutateColony` nelle azioni mutanti dei ViewModel v14 e test di disabilitazione; il salvataggio di prova e le verifiche visuali restano pendenti. [Rapporto RC16](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](validation/G1_D31_RC16_TEST_PLAN.md).
