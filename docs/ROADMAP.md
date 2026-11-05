# Aggiornamento operativo — 05/11/2026

**Passi completati:** gate `validate-f1.cmd`, incluso `validate.cmd`, verde sul workspace RC76-HF04: **885/885 test**, nessuno saltato, compilazione senza errori o avvisi, smoke nativo esistente e audit alimentari baseline/combinato superati. Applicate le richieste dell'utente su icone, intestazione, pulsanti e crescita v15 fino a ×20. [Esiti HF04](validation/RC76_HF04_ICONS_AND_GROWTH.md). Restano documentate le correzioni grafiche [HF03](validation/RC76_HF03_VISUAL_REGRESSIONS.md) e la diagnostica industriale [HF02](validation/RC76_WINDOWS_VALIDATION.md).

**Passi rimanenti:** confronto interattivo Windows della GUI (hover/focus, tooltip, scorrimento, progressi attivi) per chiudere formalmente F1-D05e; quindi RC77/E4a.3, misure a orizzonte pieno e studio E4b. I render ispezionati sono Headless/Skia; lo smoke nativo esistente usa MainWindow storico. Le richieste HF04 autorizzano il tetto di crescita ×20 e aggiornano il riferimento grafico precedente; gli altri coefficienti economici restano subordinati alle misure e decisioni previste.

I conteggi attesi e i gate indicati come non eseguiti nella pianificazione storica sotto descrivono le consegne precedenti; lo stato automatico corrente è **885/885 verificati**. Le richieste HF04 prevalgono sui riferimenti storici della GUI e del tetto di crescita.

---

## Storico RC76-HF02 — blocco compilazione prima di E4a.3

**Situazione:** RC76-HF02 candidata; 869 test attesi. Il gate eseguito su RC76-HF01 ha segnalato 8 CS1061 per `CityCatalog` non esposto dalla sessione v15. Ripristinata la proprietà pubblica che proietta lo stato corrente verso la GUI canonica. Non sono stati modificati Desktop, bilanciamento o SQLite. [Piano di validazione](validation/RC76_HF02_CITY_CATALOG_TEST_PLAN.md).

**Ordine di lavoro:** (1) compilazione e suite `validate.cmd`; (2) `validate-f1.cmd` con 869 test, audit e smoke; (3) verifica visiva GUI; (4) RC77/E4a.3, soltanto dopo la risoluzione delle regressioni della suite. RC63 rimane l'ultimo checkpoint confermato con 771 test. Gli esiti del gate precedentemente interrotto in compilazione non dimostrano il fallimento dei test o degli audit.

---

## RC76-HF01 — correzione necessaria prima di RC77

**Stato corrente:** RC76-HF01 candidata; 869 test attesi. Il gate Windows precedente si è interrotto alla compilazione con due CS0117 dei benchmark E4a: mancava un'API pubblica per il conteggio dei lavoratori v15. Corrette le chiamate e aggiunte due regressioni mirate; [piano di verifica](validation/RC76_HF01_COMPILATION_TEST_PLAN.md).

**Sequenza:** 1) `validate.cmd` e `validate-f1.cmd` sulla RC76-HF01; 2) risolvere eventuali nuovi errori senza retrocedere sulla GUI; 3) verificare visivamente la GUI; 4) solo dopo proseguire RC77 con misure quantitative dei colli di bottiglia. Le voci e i conteggi RC76 di seguito sono conservati come storico.

---

## Incremento RC76 — E4a.2c, vincoli misurabili (candidato)

Aggiunta diagnostica per carenze materiali quotate, pressione sulle capacità cittadine (Legname lavorato, Pietra, Carbone, Ferro e Oro), forza lavoro e ricerca. Report `constraints.json/csv/md` con livelli di evidenza; un magazzino pieno o un timeout scripted non dimostrano uno stallo. 10 nuovi test, **867 cumulativi attesi**, non eseguiti in questo ambiente. [Specifica](design/E4A_V15_RC76_ANALISI_VINCOLI.md) · [Test](validation/E4A_V15_RC76_TEST_PLAN.md) · [ADR 0084](adr/0084-telemetria-vincoli-benchmark-v15-senza-diagnosi-automatica-stallo.md).

---

## Incremento RC75 — E4a.2b, benchmark avanzati (candidato)

Tre scenari v15 con due strategie; la Scuola usa il contratto GUI approvato (2 Matite per studente): V15-I01 Scuola/tecnologia, V15-I02 Forgia/Spade e V15-I03 Zecca/Monete; due ordinamenti `research-first` e `bread-first`. Il runner parte senza grants e registra **tutti i materiali effettivamente pagati** e millipunti ricerca spesi; l'analizzatore non inventa tempi per scenari non completati e confronta le politiche solo dopo due completamenti a orizzonte pieno. [Specifica RC75](design/E4A_V15_RC75_INDUSTRIA_RICERCA.md) · [Piano test](validation/E4A_V15_RC75_TEST_PLAN.md) · [Manifest `not-run`](../benchmarks/e4a-v15/industrial-scenarios.csv) · [ADR 0083](adr/0083-benchmark-industria-ricerca-v15-senza-grants.md).

**Storico RC75:** 857 test erano attesi nella release RC75 (ora 867 in RC76); nessuna misura economica completa è stata eseguita. I tempi di timeout non dimostrano uno stallo strutturale.

---

# SimRisorse — Roadmap operativa dopo RC76

**Codice corrente:** RC76 / E4a.2c (candidata). **867 test attesi**: 857 RC75 + 10 nuovi casi RC76. Il gate F1 verifica la suite cumulativa 867, ma non è stato eseguito. Non sono stati eseguiti in questo ambiente. **Ultima release effettivamente validata:** RC63 (771/771 test, parità GUI approvata).

## 1. Gate di uscita F1: RC72

La RC72 non cambia il motore, la GUI, i prezzi o SQLite. Introduce quattro regressioni cumulative (due minuti di filiere miste, tick frazionari e pausa, tre riavvii SQLite) e il comando automatico `validate-f1.cmd`.

**Il gate F1-D05e rimane formalmente APERTO finché non risultano verdi:**

1. `./validate-f1.cmd`: esegue `validate.cmd` e richiede **867/867 test**, nessun fallito e nessuno skipped, validando i contatori TRX e il report del run appena creato. Controlla anche smoke nativo Windows e screenshot prodotti dai test.
2. Audit sintetico `--audit-v15 --seconds 120` e audit combinato `--audit-v15 --seconds 120 --profile combined`, con 120 secondi effettivi, nessuno squilibrio, nessuna produzione del `food-store` e tutte le sette produzioni combinate positive.
3. Riconciliazione fisica al millesimo, integrità SQLite generation 6 e tick frazionari attraverso i quattro nuovi test RC72 più le regressioni RC64–RC71.
4. Parità GUI v14/v15 tramite test Desktop RC63 e **controllo visivo manuale**, compresi edifici grigi consultabili, elenchi con icone, pulsanti storici, crescita nella sidebar, Risorse a sette colonne, Scuola/Esercito. Un test automatizzato non sostituisce l'ispezione grafica.
5. Nessun mutamento non autorizzato del salvataggio personale v14. La compatibilità con vecchi DB sperimentali v15 non è obbligatoria.

Il comando scrive i risultati in `.local-cache/m4/f1-gates/<id>/f1-gate.json`, mantenendo log, percorsi TRX e JSON dei due audit. **Non dichiarare F1 chiusa solo perché è stato scritto lo script.** È disponibile l'opzione `-Combined24Hours` per lo stress combinato di 24 ore, oneroso e non incluso nel gate minimo.

## 2. Sequenza degli incrementi

| Fase | Obiettivo | Stato e criterio d'uscita |
| --- | --- | --- |
| RC63 | Parità GUI v14 validata nella v15 | **771/771 confermati**; riferimento visivo canonico |
| RC64–RC68 | Stato/validazione/comandi civici nativi in progressione | Implementati, test cumulativi non confermati |
| RC69 | Radice `V15CityCampaignSnapshot` nativa + SQLite | Implementata, non validata |
| RC70 | Equivalenza del file SQLite temporaneo + capacità Matite v15 | Implementata, non validata |
| RC71 | Planner senza `food-store` fittizio | Implementata, non validata |
| **RC72** | Test end-to-end e gate F1-D05e riproducibile | Implementata, gate Windows non eseguito; 826 test al momento RC72 |
| **E4a.1 / RC73** | Runner v15 con partenza naturale (N01–N03), JSON/CSV e risultati non inventati | **Implementata, compilazione/test pendenti**; misure complete solo dopo gate F1 verde |
| E4a.2a / RC74 | Analisi evidence-based dei report naturali v15 | **Implementata, test pendenti**; nessuna misura completa ancora eseguita |
| **E4a.2b / RC75** | Scenari avanzati v15, due politiche, ledger materiali e ricerca | **Implementata, test pendenti**; manifest in stato `not-run` |
| E4a.2c / RC76 | Deficit misurabili e pressione capacità (non prova di stallo) | Implementata, test pendenti |
| E4a.3 / RC77 | Produzione persa e politiche controllate di recupero | Prossima: richiede validazione dei report RC76 |
| E4b.1 / RC78 | Studio quantitativo di tre curve quasi esponenziali | Parametri da approvare, non ancora implementati |
| E4b.2 / RC79+ | Costi e squadre minime per edificio | Zecca con ≥10 lavoratori/unità; verificare softlock e batch |
| M5.4–M7 | Navale, conquiste, Castello, vittoria | Dopo E4b consolidata |

## 3. Contratti tecnici e grafici non negoziabili

- La GUI v14 approvata è vincolante per v15: [ADR 0071](adr/0071-gui-v14-contratto-canonico-v15.md). Nessuna nuova schermata semplificata.
- Il Cibo pronto è solo del Deposito; Pesce e Pane possono esistere sia nella città sia nei compartimenti raw, ma come stock **distinti**. Animali e altri intermedi hanno proprietari dedicati.
- I trasferimenti non sono produzione o consumo. Pasti atomici e quota Pane per abitanti avanzati. Calcolo al millesimo.
- La v15 usa 15 famiglie produttive correnti; `food-store` non deve rientrare in checkpoint o produzione. Lo storico di importazione può continuare a contenerne il contatore.
- SQLite v15 generation 6 e salvataggio v14 separato. Incompatibilità dei DB di sviluppo consentite, mai cancellazione silenziosa di un salvataggio.
- Non cambiare prezzi, ricette, capienze e formule senza misure E4a e successiva decisione.

## 4. Rischi e criteri di blocco

| Rischio | Controllo |
| --- | --- |
| Test cumulativi RC64–RC72 non eseguiti | Gate Windows a conteggio esatto + diagnostica TRX; correggere eventuali regressioni prima di E4a |
| Audit combinato degrada o non termina | Report con errore e nessun falso successo; 24h resta opzionale |
| Formato v15 in memoria e SQLite divergono | Verifica di equivalenza staged/committed, riavvii con tick frazionari e inventari fisici |
| Scorte duplicate o `food-store` reintrodotto | Ledger al millesimo e controllo sul Cibo cittadino e produttori attivi |
| Regressione grafica simile a RC62 | Test Desktop + ispezione manuale prima di promuovere la release |
| Benchmark fittizi scambiati per partite naturali | Etichette `natural`/`controlled` e provenienza delle risorse nelle esportazioni |
| Costi e squadre minime causano stalli | Audit capacità, personale, ricette, dipendenze e strategie di recupero |

## 5. E4a.1 — Runner naturale v15 implementato nella RC73

La RC73 introduce la CLI `--benchmark-e4a-v15-natural`, i tre scenari V15-N01/N02/N03 e i report JSON/CSV. Usa una **partita v15 nuova senza stock aggiunti**, comandi effettivi e clock unificato. Le durate abbreviate sono marcate diagnostiche; completamenti non osservati rimangono nulli. I manifest si trovano in [`benchmarks/e4a-v15/scenarios.csv`](../benchmarks/e4a-v15/scenarios.csv) e mantengono tutti `not-run` fino all'esecuzione. Non sono state apportate modifiche all'economia. [Specifiche RC73](design/E4A_V15_RC73_NATURAL_RUNNER.md).

### RC74 — Analisi implementata, nessun tempo inventato

`V15NaturalProgressionAnalysis` interpreta i JSON già prodotti da RC73, classificando `diagnostic-only`, `milestone-observed`, `script-policy-timeout`, `script-policy-blocked` e `production-not-observed-within-horizon`. Registra attese e spesa parziale planks/stone. Rifiuta report contraddittori: **il timeout di una strategia non dimostra l'impossibilità economica della partita**. CLI `--analyze-e4a-v15-natural`. [Specifica](design/E4A_V15_RC74_ANALISI_PROGRESSI.md) · [Test](validation/E4A_V15_RC74_TEST_PLAN.md). E4a.2b / RC75 ha introdotto un runner indipendente per Scuola, ricerca, Forgia e Zecca con due politiche; nessuna misura completa è ancora disponibile.

### Estensioni e verifiche ancora previste in E4a.2c

Il benchmark storico v14 va tenuto separato:

Il CSV in [`benchmarks/e4a/scenarios.csv`](../benchmarks/e4a/scenarios.csv) elenca E4A-01…10, **tutti `not-run`** nel file incluso; i runner `--benchmark-e4a-natural` e `--benchmark-e4a-school` in `tools/SimRisorse.Simulation/Program.cs` sono ancora orientati alla **v14**. Gli audit `--audit-v15` misurano coerenza, **non** il tempo di raggiungimento naturale di un edificio.

### Lavori

1. Ampliare la politica RC73 per i percorsi intermedi e avanzati, mantenendo popolazione e scorte iniziali autentiche. Non far apparire edifici o materie dal nulla nei percorsi **natural**.
2. Distinguere scenari **natural** (nessun finanziamento fittizio) da **controlled** (asset aggiunti e dichiarati). I confronti tra politiche richiedono la stessa condizione iniziale.
3. Arricchire la telemetria RC73 con capacità, ricerca, rate effettivi e checkpoint; esportare JSON/CSV con versione, strategia, tempi e motivi di blocco senza inventare valori mancanti.
4. Misurare `pass`, `blocked`, `timeout`, `not-run`, `not-applicable`, **senza sostituire i fallimenti con tempi inventati**.
5. Mantenere risultati storici v14 separati dai dati v15; non chiamare v15 un report prodotto dal runner v14.

### Scenari minimi v15

| ID proposto | Categoria | Milestone/condizione di uscita | KPI chiave |
| --- | --- | --- | --- |
| V15-N01 | natural | Primo Cibo pronto prodotto e primo Deposito operativo | Tempo, risorse, lavoratori, pasti mancati |
| V15-N02 | natural | Primo Pane prodotto (RC73) | Prezzo filiera, popolazione e disponibilità scorte |
| V15-N03 | natural | Prima Selvaggina prodotta dal Cacciatore (RC73) | Costi, addetti e stock animale |
| V15-N04 | natural | Prima Scuola e primo ricercatore (futuro) | Costi effettivi, Matite, addetti e tempi |
| V15-N05 | natural | Prima tecnologia comprata e primo tier Grande (futuro) | Saldo ricerca, costi, output e stalli |
| V15-N06 | natural | Prima Forgia e prima Zecca (futuro) | Tempo reale, filiere necessarie, capacità |
| V15-N07 | natural | Seconda Zecca e popolazione per 20 addetti (futuro) | Risorse accumulate, espansione e staffing |
| V15-C01 | controlled | Dep. con 5 ingredienti alternativi simultanei | Priorità trasformazioni, sostituzioni, rese |
| V15-C02 | controlled | Pieno magazzino e input concorrenti | Blocchi, throughput, trasferimenti fisici |
| V15-C03 | controlled | Scuola/ricerca vs industria pesante | Concorrenza addetti, input e Carbone |
| V15-C04 | controlled | Salvataggi, pausa, tick frammentati | Identità checkpoint, determinismo |

I tempi massimi degli scenari v15 vanno definiti **prima** di raccogliere i risultati e versionati nel CSV corrispondente.

## 6. E4a.2 — Interpretare le misure e fissare il problema economico

### Report obbligatori

1. Sequenza dei prerequisiti di ogni edificio avanzato, con costo **diretto e indiretto** (fabbriche, persone, tecnologia, magazzini e tempo).
2. Curve di progresso: risorse accumulate/spese, produzione e saldo per secondo, ore simulate, popolazione e posti lavorativi, capacità disponibile e ore di stallo.
3. Casi di **softlock**: scorta massima insufficiente per un costo, dipendenza circolare, assenza di personale, fame, materiali che richiedono strutture inaccessibili.
4. Analisi per strategie: `growth-first`, `research-first`, `storage-first`, `industry-first`; distinguere giocata efficiente da pessima senza inventare durate.
5. Sensitività: variazione di costo/resa/occupazione; capacità di recupero dopo un investimento poco efficiente.

**Gate per E4b:** pubblicare numeri riproducibili e limiti, chiarire quali colli di bottiglia sono voluti e quali rendono la partita impossibile. I target 8 h efficienti / 32+ h lente restano **aspirazioni da verificare**, non risultati o vincoli già dimostrati.

## 7. E4b.1 — Disegnare le curve economiche (prima del codice)

### Aspetti già approvati

- Progressione quasi esponenziale **per complessità e stadio della filiera**, non soltanto rincaro delle copie della stessa famiglia.
- Prima Zecca confrontabile, come **investimento totale**, con 100–1.000 Tagliapietre base (range qualitativo); non ridurre il confronto a Legno/Pietra.
- Minimo **10 addetti per unità produttiva Zecca**; due Zecche al massimo ritmo richiedono 20 addetti.

### Alternative da simulare, non ancora approvate

- Curva A: moltiplicatori per era/famiglia con crescita moderata delle copie.
- Curva B: costo a stadi basato sulla profondità tecnologica + consumo di materiali avanzati.
- Curva C: combinazione di ingresso oneroso + costo infrastrutturale e occupazionale crescente.

Per ogni alternativa produrre prospetti del costo della prima/seconda Zecca e dei tier Base/Grande/Avanzato, investimento per lavoratore disponibile, tempo d'attesa e rischio softlock. Controllare precisione e overflow aritmetico, pagamento di lotti `+5/+10`, confronto costruzione diretta vs upgrade, storici d'acquisto non resettati da demolizione o upgrade.

**Decisione richiesta:** approvare parametri numerici solo dopo il rapporto comparativo E4a/E4b. La formula candidata è nella [specifica E4b](design/E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md).

## 8. E4b.2 — Squadre minime, moduli e capacità di lavoro

Con i tier attuali, i posti `1/3/10` **non** consentono 10 lavoratori su una Zecca Base; occorre disaccoppiare posti e moduli produttivi.

### Contratto comportamentale da implementare

- `CrewSize(mint) = 10` per unità, applicato alla sola famiglia (eventuali altre famiglie con soglia vanno motivate).
- 0–9 lavoratori: **zero Monete, zero consumo Oro/Carbone** imputabile alla Zecca.
- 10–19 lavoratori con due unità: **al massimo una unità attiva**; 20: due unità attive, subordinate a input e capacità.
- Nessun worker può alimentare due moduli. Personale parzialmente assegnato rimane disponibile **solo per quella famiglia** fino alla rimozione esplicita.
- Testare `0/9/10/19/20`, diversi tier, input vuoti, capacità piena, pausa, salvataggio e riavvio.
- La semantica candidata 1/3/10 moduli Base/Grande/Avanzato (con 10/30/100 addetti al pieno regime per Zecca) **non è ancora approvata** per ogni tier/famiglia: serve verifica di popolazione e costi.

### GUI senza redesign

Nel dettaglio esistente mostrare con gli stessi componenti: moduli posseduti, operativi, lavoratori assegnati, mancanti, collo di bottiglia e produzione nominale/effettiva. La riga di costruzione e quella del personale rimangono dove già approvate, con le stesse icone e gli stati corretti.

## 9. M5.4–M5.7 — Espansione successiva, non anticipata

1. **M5.4:** Cantiere e flotta persistente, tipi nave, costo, requisiti di equipaggio, capacità, regole di annullamento e preflight; confermare prima le scelte navali ancora aperte.
2. **M5.5:** esplorazione come attività temporizzata, separata dalla conquista; nessuna perdita irreversibile nelle prime versioni; salvataggio e ripresa deterministici.
3. **M5.6:** conquista deterministica con requisiti, preflight e risultato riproducibile; almeno un'isola obbligatoria per la progressione finale.
4. **M5.7:** Castello come obiettivo tardivo con prerequisiti economici/militari e di conquista, non mera spesa di Legno/Pietra.
5. **M6:** definire vittoria e tempi/metriche di gioco completo; **M7:** qualità, UX, accessibilità, packaging e rilascio.

Riferimenti storici: [proposta prima campagna](design/FIRST_CAMPAIGN_PROPOSAL.md), [decisioni prima di M5](design/PRE_M5_DECISION_GATE.md), [ADR 0017](adr/0017-naval-mission-boundaries-proposta.md). Non interpretare vecchie stime o stati v55 come implementazione RC68.

## 10. Metodo per ogni incremento (obbligatorio)

- **All'inizio:** riportare checkpoint validato, candidatura corrente, milestone, confini e rischio; usare i file della release precedente, non ricreare interfacce o regole già approvate.
- **Nel codice:** un tema/contratto per RC. In caso di breaking change del DB sperimentale, documentare versioni non più leggibili e fare fallire chiaramente il caricamento, senza toccare v14.
- **Test:** aggiungere test mirati, non cambiare asserzioni soltanto per ottenere verde; confrontare colonna e funzioni della GUI v14. Correggere i gate rossi prima di aggiungere una nuova funzione.
- **Documentazione:** aggiornare `CURRENT_STATE.md`, questa roadmap, `NEW_CHAT_HANDOFF.md`, eventuale ADR e `CHANGELOG.md` con fatti del codice e **stato delle esecuzioni**.
- **Consegna:** archivio ZIP **integrale** e manifest SHA-256; nessun file temporaneo, cache, vecchio manifest o salvataggio personale. Confrontare codici sorgente con la RC precedente e riportare ogni modifica.
- **Uscita:** specificare test passati dall'utente vs test soltanto attesi; gli audit statici non sostituiscono .NET/xUnit o una verifica visiva reale.

## 11. Registro dei rischi prioritari

| Rischio | Gravità | Contromisura/gate |
| --- | --- | --- |
| 35 test aggiunti RC64–68 mai validati cumulativamente | Alta | G0 prima del refactoring |
| Perdita di dettagli GUI già approvati | Alta | ADR 0071, test Desktop e confronto visivo obbligatorio |
| Conversione DTO cambia ordine o storia acquisti | Alta | Equality per ID, prove di progressi ordinati e ripresa |
| Doppia proprietà del Cibo o stock inventati dai trasferimenti | Alta | StockAuditor al millesimo, profili combinati |
| DB v15 non leggibile dopo breaking change | Media | Errore esplicito, rinomina manuale DB sperimentale, mai v14 |
| Prezzi altissimi ma capacità insufficiente | Alta | Benchmark raggiungibilità, crescita dei magazzini prima dei prezzi |
| Squadra Zecca non coerente con tier `1/3/10` | Alta | Modello moduli/posti separati, test 0/9/10/19/20 |
| Benchmark v14 scambiati per misure v15 | Alta | Runner v15 dedicato, report versionati e `not-run` finché non eseguiti |
| Roadmap diventa una cronologia di RC e perde autorità | Media | Unica roadmap attiva, cronologia nel CHANGELOG/ADR |

**Prossimo passo effettivo:** eseguire il gate cumulativo RC74 (843 test attesi) e la verifica visiva; poi misurare i benchmark naturali v15, analizzare blocchi, ampliare gli scenari E4a.2. Non iniziare E4b prima della misura E4a e non sacrificare la GUI per velocizzare l'architettura.
