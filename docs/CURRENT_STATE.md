# Stato corrente — RC40 / F1-D01 (2026-11-04)

**Decisione approvata:** riforma del Cibo con filiere Pesce, Pane, Carne (Caccia/Allevamento), Frutta, Verdura e Deposito alimentare unificato. Prima milestone F1-D01 implementata come **modello Core isolato**, non ancora collegato alla sessione. Comprende ricezione atomica, conversione a lotti/capacità/riserva Pane e 7 test. **RC40: 552 test attesi, non eseguiti qui**; **RC39: 545 test non confermati**; **RC38: 541 test confermati dall'utente**. Nessuna variazione alla GUI, cataloghi, versioni delle regole, SQLite, salvataggi, produzione corrente o benchmarking. E4b fermo.

**Prossimo F1-D02:** Stalla (Grano+Acqua → Bovini), Cacciatore (Selvaggina), Macellaio (due ricette → Carne) con nuove risorse, prerequisiti, tier, tecnologia e test. Evitare attivazioni parziali senza il percorso di migrazione e non trasformare mai automaticamente il Pane riservato in Cibo. [Roadmap](ROADMAP.md) · [Modello](design/F1_D01_RC40_MODELLO_ALIMENTARE.md) · [Piano](validation/F1_D01_RC40_TEST_PLAN.md)

---

# Stato corrente — RC39 / E4a-D02 (2026-11-04)

**RC38: 541/541 test superati secondo conferma Windows dell'utente.** **RC39 candidata: 545 test attesi, da eseguire.** Avviato il benchmark delle strategie naturali E4A-01…05, con la sessione v14 autentica e inventario iniziale invariato. La policy `natural-reference-v1` è esplicita e non ottimizzata. Esporta campioni 60 s, log di azioni riuscite/rifiutate, esiti `pass`/`timeout`/`blocked`, JSON/CSV. I cinque nomi della strategia nel manifest sono etichette di obiettivo, non cinque algoritmi distinti.

**Misure realmente eseguite qui:** nessuna. **Manifest E4a:** tutte le 10 righe `not-run`; le prove E4A-06/07 controllate restano distinte. **Nessuna modifica:** GUI predefinita v14, v13 legacy, Core, SQLite, salvataggi, economia o ricette. Prossimo gate Windows `./validate.cmd` (545 test), poi runner naturale e controllo degli esiti; E4a-D03 carbone/stoccaggio, D04 salvataggio/ripresa; E4b solo dopo dati e approvazione.

[Rapporto RC39](design/E4A_D02_RC39_BENCHMARK_NATURALI.md) · [Piano Windows](validation/E4A_D02_RC39_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [ADR 0049](adr/0049-benchmark-naturali-politica-esplicita.md)

---

# Stato corrente — RC38 / E4a-D01-F1 (2026-11-04)

**RC37 eseguita su Windows: 540/541 test PASS, uno FAIL** per `OtherBuildings` confrontati come sequenza nel test end-to-end dopo round-trip SQLite. **RC38 candidata: 541 test attesi, ancora da eseguire su Windows**. Ultima RC con suite interamente confermata: RC30 (511). RC38 corregge soltanto il confronto dei record in `V14SchoolResearchEndToEndTests.cs`, normalizzando gli insiemi per chiave (edifici, famiglie, contatori, tecnologie), e conserva l'ordine significativo degli studenti. Nessun sorgente applicativo, schema SQLite, interfaccia o coefficiente modificato.

**Prossimo gate:** `./validate.cmd`; dopo la suite verde, eseguire effettivamente i benchmark E4A-06/07. Misure ancora `not-run`; E4b sospeso.

[Rapporto RC38](design/E4A_D01_RC38_FIX_ORDERING_SQLITE.md) · [Piano test](validation/E4A_D01_RC38_TEST_PLAN.md) · [Roadmap](ROADMAP.md)

---

# Stato corrente — RC37 / E4a-D01 (2026-11-04)

**RC37 candidata: 541 test cumulativi attesi, NON compilata/testata in questo ambiente.** Ultimo gate Windows confermato RC30 (511 test); le RC31–RC37 includono le modifiche non ancora validate. La v14 è la GUI ordinaria, v13 resta recuperabile con `--legacy-v13` e salvataggi separati.

E4a-D01 aggiunge il **runner controllato per E4A-06 ed E4A-07** (una o due Scuole, due studenti, clock simulato, osservazioni ogni 30 s), output CSV/JSON e tre test. Non ci sono ancora misurazioni reali; `benchmarks/e4a/scenarios.csv` rimane `not-run`. Nessuna modifica alle regole economiche, Core, SQLite, GUI o vecchi salvataggi. **P1-D50 non è formalmente chiusa** senza il gate cumulativo; E4b resta sospesa fino alle misure e all'approvazione.

[Rapporto E4a-D01](design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) · [Piano test](validation/E4A_D01_RC37_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [ADR 0048](adr/0048-benchmark-scuole-controllato.md)

---

# Stato corrente — RC36 / P1-D50 (2026-11-04)

**RC36 candidata, 538 test attesi (2 nuovi test SQLite); compilazione e xUnit non eseguiti qui.** Ultimo checkpoint dichiarato verde dall'utente: RC30 / 511 test. RC31–RC36 sono cumulative e ancora da validare su Windows. P1-D50 rafforza compatibilità degli studenti iscritti prima delle Matite e protezione della copia valida da contatori di rimborso impossibili. Nessun cambio di codice applicativo, GUI, economia o formato SQLite.

GUI v14 predefinita; v13 solo `--legacy-v13` con slot distinto. **P1 non è chiusa** finché manca il gate Windows. **E4a ha protocollo e CSV pronti ma 0 misurazioni eseguite**; E4b rimane rinviata alle decisioni sui numeri.

[Rapporto](design/P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md) · [Piano](validation/P1_D50_RC36_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Protocollo benchmark](design/E4A_PROTOCOLLO_BENCHMARK.md)

---

# Stato corrente — RC35 / P1-D49 (2026-11-04)

**RC35 candidata**: tre test end-to-end per Scuola→Matite→Ricerca→Edifici avanzati e SQLite; nessuna modifica all'applicazione o ai coefficienti. **536 test attesi, non eseguiti**. Ultima verifica Windows confermata RC30 (511). RC31–RC34 e RC35 rimangono da validare cumulativamente. GUI v14 ordinaria; v13 `--legacy-v13` preservata; slot separati. P1-D50 prossimo, quindi E4a: misurare prima di bilanciare.

[Roadmap](ROADMAP.md) · [Piano RC35](validation/P1_D49_RC35_TEST_PLAN.md) · [Handoff](NEW_CHAT_HANDOFF.md)

---

# Stato corrente — RC34 / P1-D48 (2026-11-04)

- **Ultimo checkpoint Windows confermato:** RC30, 511 test.
- **RC31–RC34:** non ancora validate; RC34 include cumulativamente tutte le modifiche. **533 test attesi**, compilazione e xUnit da eseguire su Windows.
- **P1-D48:** inventario delle 11 tecnologie; 8 acquistabili collegate ai tier Grande/Avanzato delle 16 famiglie, 3 differite senza effetto. Validazione del catalogo su avvio/ripristino della v14; riepilogo dei benefici reali nell'Albero tecnologico. Niente acquisti di tecnologie prive di effetti.
- **Invarianti:** v14 predefinita, v13 legacy su DB separato; nessuna variazione a Matite, Scuola, costi, motore produttivo o schema SQLite.
- **Prossima:** P1-D49 integrazione end-to-end, poi D50 gate P1 → E4a misurazioni.

[Rapporto](design/P1_D48_RC34_ALBERO_TECNOLOGICO.md) · [Validazione](validation/P1_D48_RC34_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [ADR 0045](adr/0045-tecnologie-effetti-verificati.md)

---

# Stato corrente — RC33 / P1-D47 (2026-11-04)

- **Ultimo checkpoint con test confermati:** RC30, 511 previsti e superati secondo l'utente.
- **RC31 (515), RC32 (518):** non ancora testate; incluse cumulativamente nella RC33.
- **RC33 (524 attesi):** verifica unica produzione Matite fra comando e pulsante, deficit quantitativi di input/spazio per lotti, 6 test nuovi. **Non compilata né testata su Windows in questa consegna.**
- **Stabilità:** v14 ordinaria, v13 via `--legacy-v13`; nessun cambiamento di bilanciamento, salvataggi o schema SQLite.
- **Prossima:** P1-D48 tecnologie; prima o insieme alla validazione cumulativa RC33.

[Rapporto](design/P1_D47_RC33_FILIERA_MATITE.md) · [Validazione](validation/P1_D47_RC33_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [ADR 0044](adr/0044-unica-verifica-laboratorio-matite.md)

---

# Stato corrente — RC32 / P1-D46 (2026-11-04)

- **Baseline validata:** RC30, test dichiarati superati dall'utente, 511 previsti.
- **RC31:** v14 promossa a predefinita e v13 mantenuta come legacy; **515 test attesi, non verificati su Windows**.
- **RC32:** nuova visualizzazione individuale degli allievi in Scuola, senza nuove regole; **518 test attesi, non verificati su Windows**.
- **Prossima verifica:** `.\validate.cmd` sulla RC32, poi controllo GUI alle due risoluzioni e protezione dei salvataggi.
- **Roadmap:** P1-D47 Matite → D48 tecnologie → D49 integrazione → D50 gate → E4a misure → E4b bilanciamento → E4c consolidamento → M5.4–M7.

[Rapporto D46](design/P1_D46_RC32_CICLO_FORMAZIONE_SCUOLA.md) · [Piano RC32](validation/P1_D46_RC32_TEST_PLAN.md) · [ADR 0043](adr/0043-dettaglio-individuale-formazione-scuola.md)

---

# SimRisorse — RC31 / G1-D45: v14 predefinita (candidata)

**Aggiornamento 04/11/2026:** la **RC30** ha superato i test secondo la conferma dell'utente (**511 test previsti**). La GUI v14 è stata dichiarata soddisfacente e diventa l'interfaccia ordinaria; lo storico dossier di **44 prove visive non acquisite** resta disponibile ma non è più prerequisito per avviare P1. **RC31** aggiunge quattro test di routing (515 attesi) e deve ancora essere compilata/testata su Windows.

Avvio: `.\run-game.cmd` → v14, sullo **stesso** slot SQLite v14 già usato (`%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`). `.\run-game.cmd --legacy-v13` → v13, DB storico separato. `--experimental-v14` funziona ancora come alias. `--scenario ...` e `--smoke-test` rimangono legacy per compatibilità. Nessuna migrazione automatica né modifica al bilanciamento.

**Prossima milestone:** P1-D46 audit ciclo Scuola; D47 Matite; D48 tecnologie; D49 integrazione; D50 gate. Quindi E4a → E4b → E4c → M5.4–M7. [ADR 0042](adr/0042-promozione-v14-e-conservazione-slot.md) · [Rapporto D45](design/G1_D45_RC31_PROMOZIONE_V14.md) · [Piano RC31](validation/G1_D45_RC31_TEST_PLAN.md).

---

# Stato corrente — RC30 / G1-D44 (candidata: strumenti di audit)

**Ultima RC applicativa con suite Windows confermata:** RC27, 507 test passati. **RC28+RC29+RC30 cumulativa:** 511 test xUnit attesi, ancora **non verificati**. **Screenshot reali D43:** 0/44. **Gate P0 visivo aperto**; G1-D44 per anomalie applicative in attesa di evidenze.

RC30 corregge nello strumento locale due falsi positivi P0 (file PNG non validi e 44 `NA`) e aggiunge `Verify` + un auto-test PowerShell indipendente, **non contato fra i 511 test**. Non sono stati modificati Core, Desktop, xUnit, salvataggi o parametri economici. [Rapporto D44](design/G1_D44_RC30_RAFFERZAMENTO_EVIDENZE_P0.md) · [Piano test D44](validation/G1_D44_RC30_TEST_PLAN.md) · [ADR 0041](adr/0041-gate-evidenze-visive-e-NA.md).

---

# Stato corrente — G1-D43 / RC29 (strumenti di audit; candidata)

**Baseline Windows confermata:** RC27, 507 test. **Candidata cumulativa:** RC28 + strumenti G1-D43, **511 test .NET attesi, non verificati**. In D43 non sono cambiati file applicativi, test xUnit, salvataggi o coefficienti. Aggiunto uno script PowerShell indipendente per la raccolta manuale delle **44 evidenze** e il confronto HTML v13/v14, più il dossier D43. **Prove manuali realmente eseguite in questa consegna: 0/44. P0 APERTO.**

[Rapporto D43](design/G1_D43_RC29_AUDIT_VISIVO.md) · [Piano di verifica](validation/G1_D43_RC29_PIANO_EVIDENZE.md) · [Roadmap](ROADMAP.md).

---

# Stato corrente — RC28 / G1-D42 (candidata, 04/11/2026)

**Ultimo checkpoint Windows validato dall’utente:** RC27, **507 test superati**. **Nuova RC28:** 4 esecuzioni headless aggiunte (2 scenari × 2 viewport), **511 test attesi**; compilazione/xUnit e prova visiva Windows non eseguite nell’ambiente di confezionamento. Nessuna nuova funzione di gioco o modifiche economiche.

**G1-D42:** focus visibile e differenziato nelle liste della sola GUI v14, con bordo a spessore costante (nessuno spostamento quando il focus cambia) e selezione ocra preservata. Prove automatiche predisposte per menu/frecce/selezione e geometria a 1040×740 / 1240×850. La navigazione TAB/Shift+TAB e il confronto P0 delle 22 voci richiedono ancora la verifica reale su Windows.

**Prossima fase:** completare la validazione RC28, poi **G1-D43** raccolta evidenze visive delle 22 voci; G1-D44 correzioni confermate, G1-D45 chiusura gate. Successivi P1 Scuola/tecnologie → E4a misure → E4b bilanciamento approvato → E4c migrazione → M5.4–M7. La GUI v13, i DB v2/v3, le capacità, le ricette e la simulazione sono invariati.

[Roadmap](ROADMAP.md) · [Rapporto D42](design/G1_D42_RC28_FOCUS_TASTIERA.md) · [Piano RC28](validation/G1_D42_RC28_TEST_PLAN.md) · [Matrice P0](validation/G1_D39_P0_UI_MATRIX.md).

---

# Stato aggiornato — G1-D42 (sola pianificazione, 04/11/2026)

**Baseline applicativa e test Windows confermati dall'utente:** **RC27 / G1-D41: 507 test superati**. Non è stato scritto o modificato alcun codice per G1-D42; **la suite resta di 507 test**, senza nuove asserzioni. Il progetto integrale è impacchettato con documentazione aggiornata; **non è una RC28**. La verifica visiva delle 22 voci P0 a 1040×740 e 1240×850 resta **aperta**, indipendentemente dal risultato xUnit.

**Prossime attività:** D42 focus/tastiera → D43 prove visive complete → D44 fix regressioni fondate su evidenze → D45 gate P0; poi P1-D46…D50 Scuola/tecnologie → E4a analisi → E4b bilanciamento approvato → E4c migrazione → M5.4–M7. Nessun cambio dei coefficienti o dei salvataggi.

[Roadmap operativa](ROADMAP.md) · [Runbook P0](validation/G1_D42_P0_RUNBOOK.md) · [ADR 0040](adr/0040-pianificazione-post-rc27-e-gate-sequenziali.md).

---

# Stato corrente RC27 / G1-D41 (candidata)

**RC26:** ultimo checkpoint Windows confermato, **504 test passati**. **RC27:** +3 casi Desktop headless (totale **507 attesi**, build e xUnit locali non disponibili). UI-07/UI-12/UI-18: selezionando un altro edificio in v14 si riportano in cima il dettaglio e la diagnosi; nessun reset durante gli aggiornamenti dello stesso elemento e nessun focus forzato. GUI v13, simulazione, XAML grafico (a parte `x:Name`), SQLite e salvataggi invariati.

**Gate:** test Windows RC27 e 22 prove visive P0 a 1040×740/1240×850 ancora aperti. **D42:** verifica delle rimanenti voci P0 e focus/tastiera. P1 Scuola/tecnologie, E4 e navale successivi alla chiusura P0.

[Rapporto D41](design/G1_D41_RC27_AUDIT_SCORRIMENTO_DETTAGLI.md) · [Piano RC27](validation/G1_D41_RC27_TEST_PLAN.md) · [Matrice](validation/G1_D39_P0_UI_MATRIX.md) · [Roadmap](ROADMAP.md).

---

# Stato corrente RC26 / G1-D40 (candidata)

**RC25:** ultimo checkpoint Windows confermato dall’utente (501 test superati). **RC26:** +3 test Desktop, 504 attesi, compilazione e xUnit pendenti. UI-14 recupera in v14 il tempo residuo e il progresso in 1/20 di secondo della v13; UI-05 riceve test delle stime di riempimento. La GUI v13, le ricette, Core/Application, XAML, SQLite v2/v3 e la separazione dei salvataggi restano invariati.

**Gate ancora aperti:** test cumulativo RC26 su Windows, 22 verifiche P0 con evidenze alle risoluzioni 1040×740 e 1240×850; confronto pratico razioni Cibo/Pane. **Prossimo D41:** Edifici/scroll/focus e verifiche P0 residue. Nessun E4.

[Rapporto D40](design/G1_D40_RC26_FLUSSI_POPOLAZIONE.md) · [Piano RC26](validation/G1_D40_RC26_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Stato corrente — RC25 / G1-D39-F1 (candidata)

- **Ultimo checkpoint Windows confermato:** RC20 (490 test superati). **RC21–RC23** non testate singolarmente, **RC24** non ha compilato: `CS1061` in `V14BuildingUpgradeAuditTests.cs` per il metodo di estensione `MouseMove` non risolto.
- **RC25:** un solo import C# aggiunto in un test (`using Avalonia.Headless;`), in conformità ai test esistenti. Suite cumulativa **501 test attesi**, compilazione/xUnit Windows ancora da eseguire.
- **Invariati:** ogni file dell'applicazione, GUI v13/v14, SQLite, motore, simulazione, coefficienti economici e salvataggi. **P0 grafico aperto**, [matrice 22 voci](validation/G1_D39_P0_UI_MATRIX.md). E4 sospeso.

[Rapporto F1](design/G1_D39_RC25_FIX_CS1061_HEADLESS.md) · [Piano test](validation/G1_D39_RC25_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Stato corrente — RC24 / G1-D39 (candidata)

- **Ultima validazione Windows confermata dall'utente:** RC20 (490 test). **RC21/RC22/RC23:** 493/496/499 test attesi e ancora non eseguiti dall'utente. **RC24:** +2 test Desktop, **501 attesi**, da validare cumulativamente; non dichiarare verdi RC21–RC24 senza `validate.cmd` Windows.
- **UI-16/UI-20, D39-01:** allineati i suggerimenti della Scuola (fabbricazione Matite, iscrizione, annullamento) ai comandi disabilitati durante la conferma di una nuova partita; dopo Annulla tornano le indicazioni originali.
- **P0 grafico sempre aperto:** matrice [UI-01…UI-22](validation/G1_D39_P0_UI_MATRIX.md) con prove manuali non ancora registrate a 1040×740 e 1240×850; 2 nuovi test di regressione non equivalgono a una prova visiva reale.
- **Invariati:** v13, XAML di entrambe le versioni, Core, Application, Infrastructure, SQLite v2/v3, costo Matite, tutte le regole economiche; E4 e navale sospesi.

[Rapporto D39](design/G1_D39_RC24_AUDIT_SCUOLA_E_GATE_P0.md) · [Piano RC24](validation/G1_D39_RC24_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md).

---

# Stato corrente — RC23 / G1-D38 (candidata)

- **RC20:** ultimo gate test Windows confermato, 490 casi. **RC21/RC22:** rispettivamente 493/496 attesi, non verificati; **RC23:** 499 attesi (+3), non eseguiti nel confezionamento (SDK .NET 10 non disponibile).
- **Audit UI-01/UI-19:** sidebar di cinque sezioni, sincronizzazione bidirezionale menu/tab, selezioni Edifici/Risorse/Tecnologia ai tick e Save/Load. I tre nuovi test non cambiano XAML o logica del gioco.
- **P0 grafico aperto:** prove visive delle 22 voci a 1040×740 e 1240×850; E4 e navale non iniziati. Ultimi interventi RC21/RC22 ancora da compilare in Windows; nessuna falsa conferma.
- **Invarianti:** v13, 7 colonne Risorse, edifici a livelli distinti, scuola e Matite, costi/salvataggi SQLite separati preservati.

[Rapporto D38](design/G1_D38_RC23_AUDIT_NAVIGAZIONE_SELEZIONE.md) · [Piano test RC23](validation/G1_D38_RC23_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Stato corrente — RC22 / G1-D37 (candidata)

- **Ultimo gate Windows dichiarato verde:** RC20, 490 test. **RC21:** 493 attesi, non testati. **RC22:** +3 test Desktop, **496 attesi**, compilazione/xUnit pendenti. Le verifiche statiche non autorizzano un pass dei test.
- **D37 Edifici (UI-12/UI-18):** la diagnosi passa in un proprio `ScrollViewer` con `MaxHeight=104`, proteggendo visibilità delle righe Costruisci/Lavoratore e degli upgrade in presenza di testo lungo; dettaglio principale rimane scorrevole. Aggiunti due test headless a 1040×740/1240×850.
- **D37 Esercito (UI-15):** nuovo test UI per valore della barra, tempo residuo a 10 secondi e soldato pronto a 30; nessuna modifica al motore.
- **Diff applicativo:** un solo XAML v14; nessun cambiamento alla GUI v13, Core, prezzi, ricerca, SQLite o salvataggi. Documentazione aggiornata con gate P0 ancora aperto e roadmap in sei fasi.

[Rapporto D37](design/G1_D37_RC22_SCORRIMENTO_DETTAGLI_ESERCITO.md) · [Piano RC22](validation/G1_D37_RC22_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Stato corrente — RC21 / G1-D36 (candidata)

- **Ultimo checkpoint Windows confermato dall'utente:** **RC20, 490 test attesi tutti passati**. RC21 aggiunge **3 test Desktop, 493 attesi**; .NET 10/xUnit non eseguiti in questo ambiente.
- **Audit Edifici D36:** corretta la coerenza visiva dei pulsanti di upgrade con Costruisci (`IsEnabled` esplicito + stile build). Gli upgrade tecnologicamente sbloccati restano visibili ma grigi senza materiali; verde e hover ocra solo quando pagabili. Non cambiato l'algoritmo che determina pagabilità.
- **Sorgenti app modificati:** un solo file XAML della GUI v14. Nuovo file test `V14BuildingUpgradeAuditTests.cs`. Invariati GUI v13, motore, cataloghi, salvataggi, schemi SQLite e coefficienti economici.
- **Gate grafico P0:** ancora aperto (22 voci, 1040×740 e 1240×850). Prossima D37: riprendere confronto delle voci ancora aperte e controlli di scrolling/diagnostica Edifici e Esercito; poi P1 Scuola/tecnologie. E4 sospeso.

[Rapporto D36](design/G1_D36_RC21_AUDIT_EDIFICI_UPGRADE.md) · [Piano RC21](validation/G1_D36_RC21_TEST_PLAN.md) · [Audit 22 voci](validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Roadmap](ROADMAP.md).

---

# Stato corrente — RC20 / G1-D35

- **Baseline verde Windows:** RC16, 476 test. RC17 (481), RC18 (484), RC19 (487) non testate. **RC20 candidata: 490 test attesi**, di cui +3 test Desktop, non eseguiti qui.
- **Audit Risorse UI-02/UI-03/UI-04:** nuova protezione automatica per 7 colonne, scrollbar, selezione ocra, tooltip, refresh e ordine delle 17 risorse. Nessuna variazione del codice di produzione perché il confronto statico non dimostra una regressione.
- **P0 visivo aperto** su 22 voci, alle risoluzioni 1040×740 e 1240×850. **G1-D36 Edifici** segue dopo questo checkpoint. P1 Scuola/tecnologie ed E4 differiti.
- [Rapporto](design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md) · [Piano Windows](validation/G1_D35_RC20_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md).

---

# Stato corrente — RC19 / G1-D34

**Baseline Windows confermata:** RC16, **476 test superati**. RC17 (481) e RC18 (484) **non verificate**; nuova candidata **RC19: 487 test attesi (+3 Desktop)**. .NET 10 e xUnit non eseguiti nell'ambiente di confezionamento. La RC19 incorpora RC17 e RC18.

- **UI-15/UI-20:** suggerimenti Esercito allineati ai comandi durante la conferma «Nuova partita?»; quantificati deficit Spade, abitanti, reclute annullabili e capienza rimborso; corregge «1 recluta».
- Tre test, di cui uno headless Avalonia. Un solo file C# v14 di presentazione modificato. Invariati XAML, motore, regole militari, GUI v13, SQLite e salvataggi.
- **P0 visuale ancora aperto** sulle 22 voci e sulle due risoluzioni 1040×740 / 1240×850. Risorse ed Edifici restano nell'audit successivo; E4 sospeso.

[Rapporto](design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Piano Windows](validation/G1_D34_RC19_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md).

---

# Stato corrente — RC18 / G1-D33

**RC16:** ultima versione validata dall'utente, **476 test passati**. **RC17 non verificata:** 481 attesi. **RC18 candidata:** 484 test attesi, nessuna esecuzione .NET 10 nell'ambiente di confezionamento. P0 visivo **aperto**; E4 non avviato.

- G1-D33 corregge solo il testo del suggerimento di assegnazione ricercatori: quantità +1/+5/+10, posti disponibili effettivi, assenza Scuola e qualificati liberi insufficienti. I comandi mantengono i controlli originali.
- Tre test Desktop aggiunti. Nessun cambiamento alle regole di popolazione, produzione, capacità, ricerca, persistenza, GUI v13, né allo XAML della v14.
- Il pacchetto RC18 **include la protezione dei checkpoint RC17 ancora da validare**, con tutti i 5 test introdotti da RC17. Non trattare RC17 come verde.

[Rapporto](design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Test](validation/G1_D33_RC18_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md).

---

# Stato corrente — RC17 / G1-D32

**Baseline validata Windows:** RC16 (**476 test superati dichiarati dall'utente, log/TRX non allegati**). **Candidata RC17:** +5 test Desktop, **481 attesi** e non ancora compilati/eseguiti qui (manca SDK .NET 10). **P0 parità grafica:** ancora aperto. **E4:** non avviato.

- **UI-20/UI-21:** Salva, Carica e ripetizione di Nuova partita bloccati durante la conferma; annullamento ripristina la sessione precedente.
- Dopo conferma «Avvia nuova», il **checkpoint precedente resta intatto anche con autosalvataggio a minuto e chiusura**, fino a un Salva v14 riuscito; quel Save riattiva l'autosalvataggio.
- Chiudere durante la conferma equivale ad annullarla, consentendo il normale salvataggio della colonia precedente. Protezione dei DB incompatibili invariata.
- Solo **2 file C# v14 di presentazione/lifecycle** e **1 file test Desktop** modificati, oltre a documentazione. GUI v13, XAML, motore e SQLite invariati.

[Rapporto](design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md) · [Piano test](validation/G1_D32_RC17_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Audit](validation/G1_D29_AUDIT_GUI_V13_V14.md).

---

# Stato operativo — SimRisorse RC16 / G1-D31

**Data:** 03/11/2026. **Ultimo gate Windows verde comunicato:** **RC15, 473 test attesi dichiarati passati**. **RC16:** protegge tutti i comandi che mutano la colonia durante la conferma «Nuova partita?»; **476 test attesi non eseguiti qui** per assenza SDK .NET 10. **Audit visivo P0:** resta aperto.

## Cosa è implementato e cosa resta invariato

- **RC16:** l’avviso di nuova partita non lascia più operativi acquisti di edifici, personale, ricerche, Scuola/Matite o Esercito; i comandi tornano disponibili dopo «Annulla». Nessun XAML o motore modificato; +3 test Desktop. Salva/Carica restano da approfondire in UI-20/21.

- RC15 completa i dettagli «Capacità» di Municipio, Magazzino e Scuola aggiungendo le rispettive Matite; la v14 blocca Pausa/Riprendi durante la conferma di nuova partita, come la v13. Tre nuovi test Desktop.
- RC13 ha introdotto nella **sola presentazione v14** il tooltip Capacità di Matite: conta Municipio/Magazzini/Scuole realmente costruiti, mostra scorta e spazio libero; 2 test nuovi per stato iniziale e caricamento checkpoint.
- **Invariati:** `MainWindow` v13, catalogo e motore economico, scuola/formazione/ricetta Matite, XAML, SQLite v2/v3 e percorsi di salvataggio.
- **Gioco v13:** `civil-growth-13`, 16 risorse, avvio normale `./run-game.cmd`, SQLite v2; da preservare.
- **Anteprima v14:** avvio `./run-game.cmd --experimental-v14`, 17 risorse con Matite, SQLite v3 isolato, GUI pixel art classica con nuove Scuola e Albero tecnologico.
- Edifici Base/Grande/Avanzato sono righe separate; costruzione +1/+5/+10, lavoratori −1/+1/+5/+10; bloccati grigi ma selezionabili; ricerca a due decimali e acquisto condizionato ai punti effettivi.
- I valori quantitativi del prototipo non sono ancora bilanciati; **2 Matite per studente** e **1/3/10 lavoratori** sono requisiti confermati.

## Gate aperti

1. **RC16 Windows:** `./validate.cmd`, **476 test attesi**; confermare build e test, registrare conteggio e TRX.
2. **P0 UI:** completare checklist UI-01…UI-22 e confronto 1040×740/1240×850; un test verde non sostituisce l'approvazione grafica.
3. Dopo P0: verifiche complete Scuola/tecnologie, E4a numerico, E4b con approvazione costi, E4c con backup/migrazione, infine M5.4–M7.

**Prossimo passo consigliato:** validare RC16 su Windows e confrontare la GUI alle due viewport. Continuare l’audit D30+ fino a chiusura UI-01…UI-22.

[Roadmap](ROADMAP.md) · [Report RC13](design/G1_D29_RC13_AUDIT_E_TOOLTIP_MATITE.md) · [Test plan](validation/G1_D29_RC13_TEST_PLAN.md) · [Handoff](HANDOFF.md).

[Diagnosi RC14](design/G1_D29_RC14_FIX_CS0136.md) · [Piano RC14](validation/G1_D29_RC14_TEST_PLAN.md).

[Correzioni RC15](design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md) · [Piano RC15](validation/G1_D30_RC15_TEST_PLAN.md).

[Rapporto RC16](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](validation/G1_D31_RC16_TEST_PLAN.md).
