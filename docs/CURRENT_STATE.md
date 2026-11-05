# Stato corrente — RC78 / E4b.1 (06/11/2026)

**Verificato:** gate `validate-f1.cmd`, incluso `validate.cmd`, **926/926 test**, nessun fallito o saltato, build senza errori o avvisi, smoke nativo esistente, audit alimentari, recupero industriale e nuovo studio economico superati. [Prove RC78](validation/E4B_RC78_TEST_PLAN.md).

**Studio concluso:** tre curve, 81 preventivi e 18 investimenti con filiera, ricerca, personale e liquidità. Baseline naturale RC77-M01 importata e verificata, cinque output identici al replay. Suggerita A: prima filiera Zecca Base circa 325 equivalenti di lavoro Tagliapietre compresi Magazzini; B 948, C 1.405. [Rapporto e limiti](validation/E4B_RC78_CURVE_COMPARISON.md) · [Specifica](design/E4B_RC78_THREE_CURVES_STUDY.md).

**Prossimo:** decidere curva e moduli tier, poi implementare regole versionate e misurare campagne naturali nuove. Prezzi e produzione attuali restano quelli precedenti: le squadre sono una proiezione isolata. GUI, SQLite e crescita ×20 HF04 invariati. Confronto interattivo F1 ancora distinto e pendente.

Le sezioni seguenti sono storiche; per riprendere usare [handoff](NEW_CHAT_HANDOFF.md).

---

# Storico RC77-M01, misure naturali complete (05/11/2026)

**Nuove misure:** eseguiti tutti i nove run naturali con l'intero orizzonte dichiarato, senza grants: **9/9 milestone osservate**, nessun timeout/blocco/diagnostica. Pane a 481s; Estrazione I a 2520s/2580s; prime Spade e Monete a 4563s/4623s (`bread-first`/`research-first`). Replay e rianalisi: **21/21 file identici**. Manifest aggiornati con tempi effettivi e [rapporto RC77-M01](validation/E4A_V15_RC77_M01_NATURAL_MEASUREMENTS.md). Codice e regole invariati; il gate del codice sottostante resta quello RC77 indicato sotto.

**Esito nel workspace Windows:** `validate-f1.cmd`, incluso `validate.cmd`, completato con uscita 0, **899/899 test superati**, nessun fallito o saltato, compilazione senza errori o avvisi. Smoke nativo esistente, audit alimentari e sei run controllati RC77 da 120s superati. [Evidenze RC77](validation/E4A_V15_RC77_TEST_PLAN.md).

**RC77 implementata:** diagnostica opzionale del planner realmente eseguito dal clock; nominale/prodotto e deficit per solo spazio, soli input o vincoli misti. Nuova CLI `--benchmark-e4a-v15-recovery`, tre scenari controllati e due politiche ciascuno, checkpoint iniziali dichiarati, azioni reali e ricevute, riconciliazione fisica per secondo, JSON/CSV/Markdown. Quattordici nuovi test. Nessuna modifica a GUI, formule o SQLite.

**Misure controllate:** Magazzino pagato → 240 Spade o 360 Monete in 120s; due addetti aggiuntivi al Carbone → 238 Spade e 357 Monete, con un secondo di avvio. Replay: quattro file identici byte per byte. Sono esperimenti finanziati, non tempi di progressione naturale. [Specifica](design/E4A_V15_RC77_PRODUZIONE_PERSA_E_RECUPERO.md).

**Modifiche HF04 richieste dall'utente:** nuove icone Bovini/Pane/Pescatore/Tagliapietre; intestazione Abitanti, Cibo, Legno lavorato e Pietra; pulsanti «Carica» e «Salva». Crescita v15 con gli stessi bonus abitativi ma tetto elevato a **×20**: al massimo un abitante ogni 3 secondi, subordinato ad alloggi e alimentazione. Undici nuovi casi verificano rate, clock, ripresa, pausa e vincoli. Formato SQLite invariato.

**Correzioni grafiche HF03:** filtro filiere separato dalle intestazioni per evitarne la sovrapposizione; nomi italiani nelle quotazioni; testi dei materiali contenuti nelle card Scuola/Esercito. Cinque regressioni nuove e otto catture v15 obbligatorie nel gate, ispezionate a 1040×740 e 1240×850. Nessuna modifica al motore o al salvataggio.

Corretta la regressione del test sul personale: il campione a zero secondi segue già le assegnazioni a Pescatore e Deposito; il confronto con la sessione avviene ora dopo le stesse azioni (3 lavoratori liberi, contro i 5 iniziali). Il codice di gioco non è cambiato.

**Diagnostica RC76 storica:** il run V15-I01 da 120s era `diagnostic-only`. Le successive misure RC77-M01 raggiungono tutti gli obiettivi con orizzonti integrali e sono ora riportate nei manifest naturali.

**Prossimo passo:** RC78/E4b, confronto quantitativo delle tre curve candidate con costi diretti/indiretti, ricerca, storage e personale. Nei percorsi misurati la ricerca domina l'attesa e `bread-first` anticipa di 60s a parità di spesa; non è dimostrata l'ottimalità. Il requisito futuro di 10 addetti per Zecca e i coefficienti restano da studiare/decidere. Il confronto interattivo Windows rimane necessario per promuovere formalmente F1; queste misure indipendenti non lo sostituiscono.

Le sezioni seguenti conservano lo stato precedente all'esecuzione; conteggi e verifiche pendenti di allora sono superati dall'esito sopra. Per riprendere usare [handoff](NEW_CHAT_HANDOFF.md) e [rapporto Windows](validation/RC76_WINDOWS_VALIDATION.md).

---

## Storico RC76-HF02 — contratto CityCatalog (candidato al momento della consegna)

Il gate Windows sulla RC76-HF01 si è fermato alla compilazione con 8 errori CS1061: `V15FoodIndustryCampaignSession` non esponeva più `CityCatalog`, richiamato da ViewModel Desktop e test Application/Infrastructure. La RC76-HF02 ripristina la proprietà pubblica con il tipo già esistente `V15CampaignCatalogView`, calcolato dal checkpoint corrente tramite `V15CampaignCatalogProjection.Project`. Nessuna modifica ai componenti Desktop, alle regole o alla persistenza. **869 test cumulativi attesi**, invariati; nessuna verifica .NET 10 eseguita qui. [Piano RC76-HF02](validation/RC76_HF02_CITY_CATALOG_TEST_PLAN.md).

**Ultimo checkpoint integralmente validato:** RC63 (771/771). Il gate F1 resta aperto fino al completamento di compilazione, TRX, audit e verifica visiva GUI. I difetti CS0117 della hotfix HF01 sono già compresi nel codice di questa release.

---

## Hotfix corrente RC76-HF01 — compilazione benchmark v15 (candidato)

Due errori CS0117 rilevati nel gate su Windows: `V15FoodCampaignCheckpoints.BusyOutsideFood` è `internal` al Core, ma veniva richiamato da Application. La RC76-HF01 introduce `V15FoodCampaignCheckpoints.AvailableWorkers(checkpoint)` come API pubblica condivisa da sessione e runner, senza modificare il calcolo della disponibilità. Due test aggiuntivi; **869 test cumulativi attesi**, ancora da verificare. [Piano di controllo](validation/RC76_HF01_COMPILATION_TEST_PLAN.md).

**Ultima versione interamente validata:** RC63 (771 test). **Gate F1:** aperto, nessun audit di RC76-HF01 dichiarato superato. GUI validata, costi e SQLite invariati.

---

## Incremento RC76 — E4a.2c (candidata)

Telemetria dei deficit misurabili nei tentativi falliti, delle scorte e delle capacità dei materiali cittadini e analisi `measured-shortage`/`failure-code-only`/`capacity-sample`. Aggiunti 10 test, totale **867**. GUI, bilanciamento, Core e SQLite invariati. I benchmark completi non sono stati eseguiti. [Specifica](design/E4A_V15_RC76_ANALISI_VINCOLI.md) · [ADR 0084](adr/0084-telemetria-vincoli-benchmark-v15-senza-diagnosi-automatica-stallo.md).

# Stato corrente — SimRisorse RC76 (05/11/2026)

**Sorgente attuale:** RC76 / E4a.2c, candidata con **867 test cumulativi attesi** (857 RC75 + 10 casi RC76). Non è stata eseguita compilazione o suite xUnit .NET 10 in questo ambiente. **Ultima release validata:** RC63 (771/771 e GUI approvata). F1-D05e rimane aperta fino al gate `validate-f1.cmd` completo e alla verifica visiva.

## RC75 — Scuola, ricerca, Forgia e Zecca

- Creato `V15IndustrialProgressionBenchmark`: tre scenari `V15-I01` (ricerca Estrazione I), `V15-I02` (prime Spade dopo Industria I), `V15-I03` (prime Monete dopo Industria I), con due politiche `research-first` e `bread-first` per scenario. Entrambe includono Pane/Casa grande/Carbone e il crafting delle Matite: la GUI richiede 2 Matite per studente, vincolo presente anche nei benchmark. Partenza **naturale identica a RC73**, senza grants o salvataggi personali.
- Creata `V15BenchmarkFixture`, che riproduce il punto di partenza v15, e `V15IndustrialProgressionAnalysis` per interpretare JSON/eventi. Ogni acquisto registra l'intero bundle di materiali effettivamente pagati e la ricerca acquistata in millipunti: nessuna spesa attribuita a tentativi falliti, nessun costo produttivo continuo confuso con un investimento una tantum.
- CLI `--benchmark-e4a-v15-industry` con report JSON, CSV eventi/campioni, analisi JSON/CSV/Markdown e `--analyze-e4a-v15-industry` per rileggere un output senza eseguire un nuovo benchmark.
- Dati dei sei abbinamenti nel manifest `benchmarks/e4a-v15/industrial-scenarios.csv` in stato `not-run`. Un timeout/blocked di una politica non dimostra un softlock del gioco; nessuna durata di completamento non osservata viene inventata. Le differenze fra politiche sono riportate solo se entrambe completano a orizzonte pieno.
- +14 test xUnit nel progetto Application. Gate Windows aggiornato da 843 a **857**. **Nessun coefficiente economico o regola di personale modificato.**

[Specifica RC75](design/E4A_V15_RC75_INDUSTRIA_RICERCA.md) · [Test RC75](validation/E4A_V15_RC75_TEST_PLAN.md) · [ADR 0083](adr/0083-benchmark-industria-ricerca-v15-senza-grants.md).

## Eredità verificabile

- GUI v14 canonica anche in v15 (ADR 0071): righe degli edifici bloccati grigie consultabili, dettagli a icone ed elenchi, pulsanti e lavoratori nel layout già validato, crescita sotto il menu, Risorse a sette colonne, Scuola/Esercito. Nessun file Desktop/XAML alterato in RC75.
- Proprietà del Cibo pronto: nuovo Deposito; Pane/Pesce fra città e Deposito, animali e intermedi distinti. Il `food-store` storico è escluso dal clock v15.
- SQLite v15 generation 6; salvataggio v14 separato e preservato. I DB v15 di sviluppo possono essere incompatibili.
- RC72 ha predisposto il gate F1 (TRX + audit 120s base/combinato + smoke Desktop), ancora non eseguito. RC73 ha predisposto tre benchmark naturali alimentari N01-N03; RC74 l'analizzatore di quei report. Nessuna misura naturale completa di RC73–RC75 è stata validata.
- Il bilanciamento E4b resta futuro: costo avanzato della Zecca confrontabile con 100–1.000 Tagliapietre e **minimo 10 addetti per Zecca operativa**; numeri definitivi da misurare.

## Come riprendere

1. Utilizzare **solo RC76 integrale** come sorgente più aggiornato (non ripartire dai checkpoint storici).
2. Quando possibile, `./validate-f1.cmd` su Windows/.NET 10: attendersi **867/867**, nessun test saltato; verificare TRX, audit e la GUI anche manualmente. Non dichiarare F1 chiusa in anticipo.
3. In seguito, avviare un test diagnostico breve `--benchmark-e4a-v15-industry --scenario V15-I01 --policy research-first --max-seconds 120`; verificare i report. I benchmark a orizzonte pieno sono facoltativi finché non si può testare.
4. Prossimo sviluppo: **RC77**, diagnosi della produzione persa e delle strategie di recupero sulle filiere metallurgiche, poi confronto di curve di costo e squadre minime in E4b, senza alterare la GUI.

[Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md) · [Registro decisioni](AUTHORITY_REGISTRY.md) · [Scenario benchmarks](../benchmarks/e4a-v15/README.md).
