> **Aggiornamento 3 novembre 2026 — v58-RC3 (da validare).** Il gate Windows sulla RC2 ha compilato ed eseguito 299 test: 295 superati e 4 falliti. La RC3 corregge queste anomalie; richiede nuovamente `\.\validate.cmd` su Windows. Consultare [report RC3](validation/V58_RC3_299_TESTS.md), [stato](CURRENT_STATE.md), [roadmap](ROADMAP.md) e [rilascio candidato](history/releases/V58_RILASCIO_CANDIDATO.md). I paragrafi v57 sottostanti sono documentazione storica.

> **Aggiornamento 03/11/2026 · v58-RC2 (non validata):** questo documento contiene materiale del precedente checkpoint v57 e alcune parti sono storiche. Per lo stato attuale del ramo unificato leggere [docs/CURRENT_STATE.md](CURRENT_STATE.md), [docs/ROADMAP.md](ROADMAP.md) e [docs/validation/V58_RECONCILIATION.md](validation/V58_RECONCILIATION.md). Nuovo ruleset candidato: `civil-growth-13` (addestramento individuale); flotta non implementata.

# Handoff dettagliato per una nuova chat — SimRisorse

**Checkpoint v57, 28/10/2026.** Documento operativo da allegare insieme al pacchetto. **La v57 modifica la documentazione soltanto; motore, GUI, database e test sono quelli della v55.** Non è una release definitiva.

## A. Istruzioni di ripresa per l'assistente

1. Leggi nell'ordine [`../00_START_HERE.md`](START_HERE.md), [`CURRENT_STATE.md`](CURRENT_STATE.md), **questo documento**, [`AUTHORITY_REGISTRY.md`](AUTHORITY_REGISTRY.md), [`ROADMAP.md`](ROADMAP.md) e [`validation/V55_STATIC_AUDIT.md`](validation/V55_STATIC_AUDIT.md). La documentazione vecchia contiene frasi «corrente» storicamente valide **solo all'epoca**.
2. Il riferimento reale è la directory del **pacchetto più recente** estratto. Non basarti su una vecchia copia del repository o su altri progetti dell'utente.
3. Il codice giocabile corrente usa `civil-growth-12`. **Non** trasformare gli schemi navali in codice senza approvazione di N-01…N-07; la prossima versione di regole eventualmente sarà `civil-growth-13`.
4. Il primo gate è la validazione sul **PC Windows dell'utente**: qui manca il .NET SDK. Confronta qualunque nuovo log con i **287 casi xUnit attesi**; non proclamare superati test non eseguiti.
5. Preserva le partite personali, i benchmark storici, la politica S1 e gli ID dei cataloghi; usa soltanto file isolati per test e conversioni.

## B. Progetto, scopo e architettura

È un gestionale coloniale 2D *senza mappa di costruzione*, sviluppato in **C# / .NET 10**, **Avalonia 12.1.4**, **SQLite 3**, con UI retro 8-bit e font monospace. Si gestiscono strutture e personale tramite elenchi; la simulazione procede durante il gioco e si arresta in pausa, senza recupero del tempo passato a programma chiuso. Scorte e capacità sono ricavate dal Core, non dall'interfaccia.

| Cartella/file | Responsabilità |
| --- | --- |
| `../src/SimRisorse.Core/` | Quantità esatte, costruzioni, lavoro, popolazione, militari, invarianti |
| `../src/SimRisorse.Application/` | Scenario corrente, conversioni S1, comandi/read model |
| `../src/SimRisorse.Infrastructure/` | Checkpoint SQLite, backup, atomicità |
| `../src/SimRisorse.Desktop/` | UI Avalonia, icone, tooltip e test visuali |
| `../tests/` | Core/Application/Infrastructure/Desktop xUnit |
| `../tools/SimRisorse.Simulation/` | Prove e benchmark automatizzati senza interfaccia |
| `../scripts/validate.ps1` e `../validate.cmd` | Gate completo Windows |
| `../docs/validation/` | Rapporti per singola milestone; non tutti descrivono il presente |

La prima campagna prevede **22 edifici-obiettivo**, tutti necessari per vincere, **Castello conclusivo dopo almeno una conquista**. **Municipio** è una struttura aggiuntiva obbligatoria nello scenario, non conta tra i 22. In v55/v57 sono presenti **20 tipi campagna + Municipio = 21 strutture** e **16 risorse**; mancano Cantiere e Castello. Elenco completo di ID e nomi in [CATALOG_CURRENT.md](CATALOG_CURRENT.md).

## C. Regole già operative: fotografia non negoziabile

### Nuova colonia

- 8 persone; **Legno lavorato 120** (chiamato «Tavole» in alcuni studi), **Pietra 80, Cibo 30**, altre risorse economiche inizialmente a zero.
- Edifici iniziali: **Taglialegna (`tree-feller`), Segheria (`sawmill`), Tagliapietra (`stonecutter`), Casa piccola e Municipio**, uno ciascuno; i primi tre partono con un addetto.
- Municipio: **1 sola unità permanente** con 0 lavoratori, **+1.000 capacità a ognuna delle 16 risorse**. Non costruibile, non addestrante, non demolibile, niente controlli lavoratori.
- Base-capacity invisibile: **0**. Un Magazzino **+500 a tutte le risorse**. Ogni produttore **+50 alla risorsa prodotta**; le case aggiungono capacità specifica a **Cibo e Pane** (+10 piccola, +25 grande per ciascuna); la Caserma **+20 Spade**. **Il Pane non è un caso speciale** e non esistono magazzini privati: tutto converge nell'inventario unico della colonia.
- All'avvio: capacità Tronchi/Tavole/Pietra **1.050**, Cibo/Pane **1.010**, in genere **1.000** alle altre risorse.

### Produzione e abitanti

- Filiera legno e pietra; Pesce → Cibo; Fattoria/Pozzo/Mulino/Forno → Pane.
- Casa piccola **8 posti**, Casa grande **32**, avanzata richiede Forno. Crescita base **1 persona ogni 60 s validi** con bonus delle case fino a **2×**. Popolazione e militari condividono alloggi e razioni.
- Fonderie e miniere Ferro/Carbone/Oro: gestione della contesa del Carbone; Forgia produce **2 Spade/s** da **1 Ferro + 1 Carbone/s**; Zecca **3 Monete/s** da **1 Oro + 1 Carbone/s**. Gli input/outputs sono per addetto.
- Caserma: **280 Tavole + 200 Pietra**, Forgia prerequisito, 2 posti addestramento, **60 s per gruppo**, **2 Spade per recluta**, coda FIFO; annullamento possibile solo per reclute in coda con rimborso se c'è spazio; congedo soldati pronti senza Spade restituite; niente perdite definitive.
- Le Monete **non** sono valuta richiesta per ogni edificio; oggi sono una scorta prodotta, con possibili usi futuri da approvare.

### Interfaccia approvata e corretta

- Sidebar **Edifici / Risorse / Esercito**. Esercito nascosto nello scenario storico `economy-foundation`; salvataggio separato per scenario.
- Nella tabella Risorse: alfabetico italiano; selezione ocra; colonna **«Si riempie tra»** (`hh:mm:ss` se saldo positivo e capienza stimabile); valori numerici allineati a destra; sola quantità mostrata arrotondata all'unità, senza alterare il valore Core; tooltip sulla **Capacità** che spiega i contributi degli edifici.
- Scrollbar verticale **affiancata** e non sovrapposta alle colonne sia in Risorse che Edifici, a **1040×740 e 1240×850**; test da rieseguire sul runtime reale.
- Dettaglio edificio: costi, produzioni, consumi, capacità con **icone 14 px e testo 12 px** coerenti; diagnosi esplicita di scorte insufficienti, priorità e spazio esaurito. Municipio senza pulsanti Costruisci/assegna/rimuovi addetti.

### Salvataggi

- `civil-growth` usa SQLite `user_version=2`, snapshot v2, uno slot per scenario, save periodico/manual/chiusura; la Foundation conserva la sua generazione precedente.
- Versioni di regole disponibili **v1…v12**; conversioni S1 progressive, con backup dell'originale *prima della prima riscrittura*. Stesso gioco e scorte, **nessun regalo di scorte a ogni riapertura**.
- Passo `-11→-12`: aggiunge `town-hall` a **1** e nessun addetto, senza duplicare dotazioni. Non perdere coda/corso/soldati, lavoratori, quantità, tempo e crescita frazionaria; la v10 introdusse la riga militare obbligatoria, preservata fino alla v12.
- Le factory per regole precedenti (`CreateLegacyForBenchmarks`, `CreateAcceleratedForBenchmarks`, `CreateIronForBenchmarks`, `CreateGoldForBenchmarks`, `CreateForgeForBenchmarks`, `CreateMintForBenchmarks`, `CreateBarracksForBenchmarks`, `CreateUniformStorageForBenchmarks`) vanno preservate. **Non** allineare i benchmark storici al catalogo nuovo.
- Partita Windows personale: `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`. Non sovrascriverla, importarla o cancellarla durante diagnosi o gate; lavorare in cartelle di test.

## D. Stato delle verifiche, senza ambiguità

**v55:** audit statico realizzato; XML/XAML valido, controlli XAML richiamati dai test presenti, 38 pittogrammi corretti, riferimenti ai progetti presenti e link Markdown validi. È stato rinforzato `GameScenario` per edifici fissi, verificato nel codice il doppio caricamento SQLite post-migrazione e nascosti i comandi personale del Municipio. La **v57 non cambia niente di ciò**.

**Non certificato:** compilazione Release, 287 xUnit, benchmark, simulazioni, smoke test e render reali per la v55/v57. Qui manca `.NET SDK`; non confondere la verifica statica con un gate completo. Casi Windows precedenti, inclusi bug `CS0136`, `CS8629`, `xUnit1051`, font test e visibilità Esercito, sono riportati nel changelog come cronologia, non come bug ancora certamente presenti in v55.

Comando utente dalla root estratta:

```powershell
.\validate.cmd
```

Se passa, chiedere/conservare il riepilogo con numero di test, simulazioni e smoke. Se fallisce, lavorare **sul primo errore reale**, citare file e linea, correggere con test di regressione e preparare un nuovo pacchetto. La verifica visuale manuale delle schermate va fatta a **1040×740** e **1240×850** con selezione, scrollbar, hover, Municipio e schede.

## E. Decisioni già acquisite dall'utente

1. Prima campagna **22 tipi** fra quelli selezionati, resto escluso; possederli tutti per vincere, **Castello ultimo**.
2. Almeno **una conquista di isola obbligatoria** sblocca il Castello; missioni/isole ancora da dimensionare.
3. Continuità salvataggi **S1**, con migrazioni esplicite, copia di sicurezza e righe storiche conservate.
4. Parametri **A** di Ferro, Oro, Forgia, Zecca e Caserma; M-01…M-04 sul trattamento di reclute, soldati, razioni e congedi.
5. Grafica retro 8-bit monospace e elenchi, icone e diagnosi leggibili; niente posizionamento su griglia in questa fase.
6. Uniformità di stoccaggio **Pane come le altre risorse**, capacità spiegata nel tooltip, deposito distribuito tra Municipio e produttori/abitazioni/magazzini.
7. **Carne fuori prima campagna**, demolizione rinviata. Le ipotesi di rimborso non sono regole.

## F. Scelte che NON sono state acquisite

Tutte le opzioni **N-01…N-07** in [`design/M5_4_DECISION_GATE_V55.md`](design/M5_4_DECISION_GATE_V55.md) richiedono conferma. Proposta A, *non approvata*:

| ID | Ipotesi navale da sottoporre all'utente |
| --- | --- |
| N-01 | Navi con identità persistente |
| N-02 | Cantiere 320 Tavole + 220 Pietra; prerequisito Zecca; 0 addetti |
| N-03 | Nave 160 Tavole + 120 Monete; acquisto immediato e atomico |
| N-04 | Trasporto di 5 soldati addestrati per nave; niente equipaggio civile separato |
| N-05 | Navi riutilizzabili; nessuna perdita in M5.4 |
| N-06 | Almeno un Cantiere; nessun limite di navi per Cantiere |
| N-07 | Navi mercantili e commercio rinviati |

Se approvata integralmente: prima nave e Cantiere **480 Tavole, 220 Pietra, 120 Monete** oltre ai prerequisiti. Questo è un calcolo **teorico**, non dimostrazione di raggiungibilità. Navi come entità persistenti, **non come diciassettesima risorsa**. La versione futura diventerebbe **`civil-growth-13`** e farebbe salire il catalogo a 22 strutture totali (21 obiettivi+Municipio), poi a 23 con il Castello (22 obiettivi+Municipio).

Non avviare la codifica per una risposta generica «continua». La condizione è **gate positivo + approvazione esplicita o modifiche indicate dall'utente**.

## G. Roadmap e prossima sequenza

| Fase | Stato | Primo passo utile |
| --- | --- | --- |
| M0–M4 e M5.1 | Milestone implementate e verificate per gli incrementi storici | Preservare regressioni |
| M5.2 Forgia/Zecca | Implementate | Conservare benchmark contesa e fabbisogno |
| M5.3 Caserma/Esercito | Implementata in v55; gate corrente non riacquisito | Validare la versione corrente |
| **M5.4 Cantiere/Flotta/preflight** | **Proposta N-01…N-07 aperta** | Chiedere una decisione solo dopo il gate |
| M5.5–M5.7 | Missioni, conquista, Castello da progettare | Contratti e progressione riproducibile |
| M6 | Vittoria/classifica e bilanciamento di tutta la partita | Misurare obiettivi 8h/32h+, non inventare risultati |
| M7 | Polishing, accessibilità, distribuzione | Prove GUI DPI e qualità |

Se approvato il modello navale, lavorare con **incrementi piccoli**: **M5.4a** Core+Application e test; **M5.4b** SQLite+conversione+Desktop e test; **M5.4c** solo preflight teorico, **senza far partire missioni**; poi M5.5.

## H. Formato di avanzamento richiesto dalla collaborazione

In ogni risposta di sviluppo riportare sempre:

1. **Stato** attuale, pacchetto, ruleset, ultimo gate confermato.
2. **Realizzato** nello specifico incremento, inclusi file/moduli e rischio su salvataggi.
3. **Verifiche** eseguite e non eseguite (compilazione, xUnit, benchmark, visuali), senza dichiarazioni presunte.
4. **Prossimo passo e roadmap** fino alla vittoria, con dipendenze.
5. **Decisioni ancora aperte** e motivi per cui serve eventuale conferma dell'utente.

Aggiornare sempre `CURRENT_STATE.md`, `HANDOFF.md`, `ROADMAP.md`, `CHANGELOG.md` ed eventuali ADR/rapporti all'evolvere del codice. Se si deve passare a un'altra chat, riemettere un pacchetto completo e questo handoff aggiornato, senza perdere informazioni o ripartire da M0.
