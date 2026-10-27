# Roadmap operativa di SimRisorse

Aggiornata il **2026-10-27**. **M4.11** introduce il prototipo 8-bit; **M4.12** il lettore dei benchmark; **M4.13** l'audit aritmetico delle abitazioni; **M4.14** la [scheda decisionale D0](design/D0_DECISION_BOARD.md); **M4.15** la [prefattibilità M5.4 navale](design/M5_4_NAVAL_PREFEASIBILITY.md). Il gate Windows e la lettura quantitativa sono ora eseguiti. Questo documento distingue implementazione, verifica e decisioni aperte. Lo storico è nel [changelog](../CHANGELOG.md); il punto di ripresa è in [HANDOFF.md](HANDOFF.md).

**Ultimo esito verificato su Windows:** `.\validate.cmd` superato con **206/206 test**, zero avvisi/errori, benchmark e prove native di entrambi gli scenari completati. M4 chiusa; M5.1b Oro integrata e rifiniture dell'interfaccia verificate. [Report corrente](validation/DESKTOP_UI.md). Gli stati pendenti nelle sezioni storiche descrivono le fasi precedenti.

## 1. Stato attuale

**Aggiornamento corrente:** M4 chiusa; D0.1/D0.2/ruolo D0.3, S1 e base grafica scelti. Configurazioni A di Ferro e Oro approvate con risposte distinte. **M5.1b verificata**, `civil-growth-7`, **17 edifici / 14 risorse**, **206/206 test**, 19 render e smoke superati. Menu laterale da 240 px, card abitanti, dettaglio costi/produzione e hover ocra adottati. Prossima M5.1c scala/contesa; Forgia e Zecca richiedono i loro parametri e contratti.

| Milestone | Risultato | Stato |
| --- | --- | --- |
| **M0** | Riferimenti, organizzazione, documentazione | **Completata e verificata** |
| **M1** | Prova dello stack .NET/Avalonia/SQLite | **Completata e verificata** |
| **M2** | Motore economico indipendente dalla GUI | **Completata e verificata** |
| **M3** | Prima partita desktop salvabile | **Completata e verificata** |
| **M4** | Crescita civile, abitazioni e Pane | **Chiusa e verificata; demolizione rinviata** |
| **M5** | Economia avanzata, esercito e isole | **M5.1a Ferro e M5.1b Oro verificate; prossima scala/contesa** |
| **M6** | Vittoria, risultati e bilanciamento globale | **Da avviare dopo M5** |
| **M7** | Qualità finale e distribuzione | **Da avviare dopo M6** |

**Baseline corrente:** C#/.NET 10, Avalonia 12.1.4, SQLite v2; regole `civil-growth-7`, **17 tipi di edifici**, **14 risorse**. GUI retro monospace accettata; costruzioni tramite elenco o schede, **senza mappa di posizionamento**.

**Validazione corrente:** esecuzione Windows `899f6ff3e8ff41288330c3d26b4c4ea7`, SDK 10.0.201, **206 test superati** (Core 67, Application 61, Infrastructure 30, Desktop 48), zero avvisi/errori, 17 render standard più due del dettaglio e smoke nativi superati. I precedenti gate restano nello storico; i 204 test del [rapporto Oro](validation/M5_1B_GOLD.md) descrivono il gate di integrazione precedente.

**Scelte ricevute:** [prima campagna con 22 tipi](design/FIRST_CAMPAIGN_PROPOSAL.md), 17 presenti e 5 futuri; altri 21 riferimenti fuori. Possesso dei 22 tipi e Castello dopo una conquista obbligatoria. Base grafica approvata, demolizione rinviata, S1 e configurazioni A di Ferro/Oro scelte. Parametri di Forgia/Zecca e contratti militari/navali restano distinti.

**Evidenza preparatoria M5.1:** [prova industriale A/B separata](../experiments/industry-probe/README.md) eseguita: otto confronti con acquisti in blocco/progressivi, due modalità di avanzamento, saturazione e ripresa delle fonderie. Ipotesi misurate in una colonia sintetica, separate dai parametri poi scelti per il gioco. Gate preparatorio di allora `4c51d35a1c924fdca6656687e8f21e0e`: **177/177 test**, simulazioni e smoke superati.

## 2. Regole di pianificazione

1. **Prima le regole, poi il codice:** costi, ricette, obiettivi, limiti e gestione dei casi eccezionali richiedono una scelta esplicita prima di diventare meccaniche.
2. **Incrementi verticali piccoli:** una funzionalità comprende motore, lettura, persistenza quando necessaria, test, simulazione e documentazione.
3. **Tre stati distinti:** *proposto*, *implementato ma da validare*, *verificato*. Un test scritto non è un test superato; una proposta tecnica non è una regola approvata.
4. **Versioni delle regole esplicite:** una modifica economica o di compatibilità richiede una nuova versione e una politica di conversione dei checkpoint. Le analisi e i read model non richiedono una nuova versione.
5. **Bilanciamento progressivo:** misurare costi, tempi e carenze a ogni nuova filiera, poi misurare la durata complessiva in M6. Le **8 ore efficienti / 32+ ore inefficienti** sono obiettivi del gioco completo, non certificazioni di M4.
6. **GUI progressiva:** estendere la base 8-bit già approvata durante M5–M6, rifinire in M7. La GUI non replica formule economiche del Core.
7. **Salvataggi protetti:** test e benchmark non toccano le partite personali; le conversioni sono verificabili e preservano l'originale secondo le regole già adottate.

## 3. M0–M3 — Baseline completata

Non riaprire queste milestone per sviluppi ordinari: le successive modifiche sono nuove funzionalità o manutenzione regressiva.

| Milestone | Evidenze principali | Riferimento |
| --- | --- | --- |
| M0 | Riorganizzazione e integrità dei riferimenti; documentazione e ADR | [Handoff](HANDOFF.md) |
| M1 | 23 test della prova ridotta, stack adottato | [Verifica M1](validation/M1_STACK_PROBE.md) |
| M2 | 45 test e simulazioni del motore autonomo | [Verifica M2](validation/M2_CORE.md) |
| M3 | 82 test, desktop, ciclo di salvataggio e ripresa | [Verifica M3](validation/M3_DESKTOP.md) |

I conteggi descrivono i checkpoint di uscita di ogni milestone, **non** il numero totale della suite attuale.

## 4. M4 — Chiudere la progressione civile

**Obiettivo:** un'economia civile riproducibile con più livelli abitativi, doppia alimentazione, filiere recuperabili e diagnostica interpretabile, pronta come base del gioco avanzato.

| Fase | Incremento | Stato | Criterio verificabile |
| --- | --- | --- | --- |
| **M4.1** | Case piccole, Cibo e crescita | Verificata | Arrivi dopo secondi validi, carenze e case piene corrette |
| **M4.2** | Magazzini e capacità derivata | Verificata | Espansione e ripresa della produzione; salvataggi convertibili |
| **M4.3** | Fattorie, Pozzo, Mulino e Forno | Verificata | Ricette e prerequisiti; cronologia del Pane |
| **M4.4** | Casa grande, Pane distinto, razioni avanzate | Implementata; gate riferito superato dall'utente | Crescita 16→17, doppia razione e conversioni `-1/-2/-3 → -4` |
| **M4.5** | Benchmark abitazioni e scala Pane | Gate riferito superato dall'utente | Risultati riproducibili e riconciliazione di ogni passo |
| **M4.6** | Diagnostica numerica Forno + intera filiera | **Gate v17 riferito superato dall'utente** | 163 test attesi, valori per Grano/Acqua/Farina/Pane, UI compatta |
| **M4.7** | Confronto filiera snella/completa a 220 abitanti | **Gate v17 riferito superato dall'utente** | Quattro strategie, costi reali, filiera operativa e 1.800 secondi riconciliati per strategia |
| **M4.8** | Consolidamento M4 e stress oltre 308 abitanti | **Gate v21 riferito superato dall'utente** | Due profili di scala, limiti documentati, baseline fissata |
| **M4.9** | Ergonomia GUI: avvisi fissi, costruzione, densità, geometria hover | **Gate v23 riferito superato dall'utente** | Prerequisiti visibili senza scroll, colori distinti e posizioni invarianti in hover alle due risoluzioni |
| **M4.10** | Case che accelerano gli arrivi, persistenza frazionaria e conversione da `-4` a `-5` | **Gate v24 riferito superato dall'utente** | Bonus +15%/+30%, limite 2×, stessi risultati con passi diversi, vecchi salvataggi v2 leggibili, benchmark e GUI coerenti |
| **M4.11** | Primo prototipo desktop retro 8-bit | **Gate tecnico verificato; giudizio utente aperto** | Icone originali 8×8 per 12 edifici e 9 risorse, leggibilità e geometria a 1040×740 e 1240×850, nessun effetto sulle regole |
| **M4.12** | Rapporto comparativo dei quattro benchmark M4 | **Eseguito e risultati consolidati** | Lettura JSON reale, confronti quantitativi M4, controlli di versione/coerenza, nessuna mutazione al gameplay |
| **M4.13** | Audit economico dell'incentivo numerico delle case | **v27 analisi completata, benchmark da eseguire** | Enumerazione delle quattro combinazioni a costo minimo per 2×, costi alimentari e condizioni in cui il bonus non basta |
| **M4.14** | Preparazione del gate D0 e verifica della coerenza documentale M5 | **v28 scheda completata, decisioni non approvate** | Otto scelte tracciate, criteri di approvazione M4→M5, `civil-growth-5` baseline e S1/S2 chiariti |
| **M4.15** | Prefattibilità M5.4: Cantiere navale, flotta e trasporto | **v29 studio completato, contenuti non approvati** | Primo viaggio raggiungibile, identità navi, pagamento, save e futuro preflight distinti dalle missioni |

### M4.6–M4.7 — Gate precedente confermato dall'utente

La validazione di **v17** è stata dichiarata superata dall'utente. Il pacchetto comprendeva build Release, **163 casi xUnit attesi**, analisi dei due cataloghi, dodici cronologie standard, benchmark abitativo e Pane a quattro strategie, **nove render** e ciclo di salvataggio nativo. Gli output grezzi (`validation.json`, TRX, JSON, PNG) non sono allegati qui: non trasformare la conferma in risultati quantitativi inventati. L'assenza di .NET nell'ambiente di preparazione non autorizza una seconda certificazione autonoma.

### M4.7 — Esperimento economico senza cambiare le regole

**Implementazione confermata dal gate utente v17; output numerici non allegati.** Il comando `--benchmark-bread-scale` confronta da una colonia sintetica identica da **220 abitanti**, per 30 minuti di gioco:

| Strategia | Potenziamento | Personale in più | Legno lavorato in più | Pietra in più |
| --- | --- | ---: | ---: | ---: |
| Una filiera | Nessuno | 0 | 0 | 0 |
| Due complete | 2 Fattorie + Pozzo + Mulino + Forno | 5 | 420 | 280 |
| Seconda completa tardiva | Stesso potenziamento al minuto 15 | 5 | 420 | 280 |
| **Snella** | 1 Fattoria + Pozzo + Mulino + Forno | **4** | **360** | **250** |

Sono ora registrati abitanti finali, tempi per 240/250 abitanti, secondi bloccati, Pane/Cibo finali, lavoratori, attività dei quattro anelli, Pane prodotto/consumato e riconciliazione secondo per secondo. Il limite medio teorico **4,5 Pane/s** della strategia snella **non è ancora una prestazione osservata**; lotti produttivi e capacità di deposito possono modificarne il rendimento reale. Dettagli in [capacità della filiera](scenarios/BREAD_CHAIN_CAPACITY.md) e [benchmark attuale](scenarios/BREAD_SCALE_BENCHMARK.md).

### M4.8 — Stress ad alta popolazione e uscita dalla milestone

Il profilo da 220 abitanti arriva al massimo a 250, ancora sotto il punto in cui la filiera snella da 4,5 Pane/s diventa nominalmente insufficiente. Il comando `--benchmark-bread-high-density` confronta quattro strategie su **320→350 abitanti**, 360 posti, quattro filiere di Cibo e stessa produzione Pane iniziale. Il gate del pacchetto che lo contiene è **riferito superato dall'utente**; senza i JSON non possiamo stabilire tempi, popolazioni finali o durata effettiva delle carenze. [Scheda tecnica e limiti](scenarios/BREAD_HIGH_DENSITY_BENCHMARK.md).

- [x] Gate v17 con diagnostica e quattro strategie da 220 abitanti riferito superato dall'utente (output grezzi non allegati).
- [x] Gate Windows del nuovo profilo da 320 abitanti **riferito superato dall'utente**; output JSON non fornito.
- [ ] Politiche abitative/Persistenza/razioni senza regressioni; nessuna partita personale alterata.
- [ ] Limiti di produttività e principali strategie documentati con misure, non solo formule.
- [ ] Carenze recuperabili per i casi previsti; stato di blocco leggibile dalla UI.
- [ ] Catena e catalogo raggiungibili; baseline di regole e salvataggi fissata.
- [ ] Questioni **Carne** e **demolizione** classificate esplicitamente come *approvate*, *rinviate* o *fuori perimetro di M4*.

### M4.9 — Compattazione GUI (v22) e regressione hover (v23)

- In **Edifici**, la diagnosi dei prerequisiti e dei blocchi è ora nel riquadro **fisso sopra il comando Costruisci**; nome, descrizione e ricetta rimangono nel pannello scorrevole. Non occorre scorrere il dettaglio per vedere «Per costruire servono prima…».
- Margini, card risorse, elenco edifici, pulsanti e footer occupano meno altezza, conservando nomi e informazioni economiche. Il futuro tema pixel art 8-bit non viene ancora introdotto.
- Il pulsante **Costruisci** ha stati espliciti normale, hover, premuto e disabilitato. L'hover è verde più luminoso con bordo, il disabilitato grigio; il comando rimane disabilitato in presenza di vincoli.
- **Gate v22:** l'utente ha confermato superati i test e la validazione del pacchetto con **166 casi previsti**; gli output grezzi non sono disponibili qui.
- **Correzione v23:** lo spessore del bordo era differente in hover (**2**) rispetto al disabilitato (**1**), causando un ricalcolo dell'altezza del pulsante e uno spostamento degli elementi che lo precedono. Ora è **1 in tutti gli stati**; cambiano solo i colori. Il caso Desktop confronta geometria di pulsante, costo e quantità prima/dopo hover e disabilitazione a **1040×740** e **1240×850**. Il precedente test a una risoluzione è diventato una teoria a due casi: **167 xUnit attesi** (+1), gate successivamente riferito superato dall'utente.

### M4.10 — Case che accelerano la crescita (v24)

- **Formula adottata:** moltiplicatore `min(2; 1 + 0,15 × max(0, case piccole − 1) + 0,30 × case grandi)`. La prima Casa piccola è la dotazione iniziale e non dà bonus. Il tetto corrisponde a un arrivo ogni 30 secondi validi, contro 60 al ritmo base; gli arrivi avvengono solo a fine passo di un secondo.
- **Il contatore è unico per colonia.** Edifici nuovi cambiano il ritmo dei progressi futuri senza azzerare quanto già maturato. Cibo, Pane dei residenti avanzati, posti liberi, pausa e assenza di progresso offline restano vincoli invariati.
- **Precisione e salvataggi:** Core usa interi in ventesimi; salva secondi completi e una frazione 0–19 in tabella SQLite compatibile con i precedenti snapshot v2. Conversione `civil-growth-4 → civil-growth-5` senza resettare i secondi precedenti, con backup automatico del vecchio file alla prima sovrascrittura.
- **Gate v24:** 174 casi xUnit attesi, nuovi test su bonus, tetto, cambi di ritmo, carenze, persistenza e UI, cronologie M4 storiche preservate con `--rules legacy`, nuovo confronto `--benchmark-housing-accelerated`. **L'utente riferisce superati test e validazione Windows;** TRX/JSON numerici non allegati. Lo stato v24 è pertanto *verificato secondo l'utente*, non rieseguito in questo ambiente.

### M4.11 — Primo prototipo visivo 8-bit (v25)

- Creato un controllo `PixelGlyph` che disegna **pittogrammi originali su una matrice 8×8**, senza librerie né immagini esterne. Tutte le nove risorse e i dodici edifici civili hanno una chiave grafica stabile corrispondente all'ID di catalogo.
- Inseriti i simboli negli **edifici**, nelle **tre scorte in evidenza** e nella **tabella Risorse**. Pannelli a spigoli retti, bordi discreti e palette ridotta con blu notte, crema, azzurro e colori di produzione. Le etichette testuali e le quantità restano sempre esplicite.
- Conservati pannello di blocco sopra `Costruisci`, densità della schermata, stato disabilitato distinto dall'hover e **spessore invariato del bordo durante l'hover**. Nessun cambiamento a Core, Application, prezzo, popolazione o SQLite.
- Nuova prova di copertura pittogrammi e **due test di layout a 1040×740 / 1240×850**; conteggio complessivo atteso **177**. Da eseguire Windows `validate.cmd`, poi **ispezionare a vista i PNG** per leggibilità di icone e testi a entrambi i formati, inclusi Pane e diagnostica.

**Confine:** è una **schermata campione**, non un'approvazione definitiva di ogni colore/font/sprite. D0.7 resta aperta per revisione grafica, contrasto, scaling e DPI prima dell'estensione a M5.

### M4.12 — Rapporto leggibile dei benchmark (v26)

- Aggiunto `scripts/report-m4.ps1`: legge gli esperimenti che `validate.cmd` ha già prodotto e genera una **tabella affiancata** di tempi e risparmi delle case accelerate, più due tabelle di resa della filiera Pane (220 e 320 abitanti).
- Valori ricavati **esclusivamente dai JSON effettivi**, senza attribuire risultati al solo gate passato. Controlli su durate, versioni, ID e secondi riconciliati; assenza dei file segnalata come errore, non interpretata come dato zero.
- Uso documentato in [Lettura benchmark M4](validation/M4_BENCHMARK_READER.md). Script indipendente: non richiede nuove dipendenze .NET e non modifica salvataggi o checkpoint. Manca la prova PowerShell Windows, da fare quando disponibile insieme al gate v25.
- Nessuna nuova economia introdotta. Il passo successivo, dopo aver visto i valori reali, è chiudere formalmente M4 e decidere D0.1–D0.8 con l'utente.

### M4.13 — Incentivo numerico e costo delle abitazioni (v27)

- Nuovo [audit delle case](scenarios/HOUSING_INCENTIVE_AUDIT.md), derivato dalle formule effettive di Core e dai costi di scenario, senza eseguire la simulazione: il tetto **2×** si raggiunge con sette Case piccole aggiuntive, oppure cinque piccole e una grande, tre piccole e due grandi, una piccola e tre grandi.
- **Tutte e quattro costano 560 legno lavorato e 280 pietra per le sole abitazioni**, ma offrono rispettivamente **64, 80, 96 e 112 posti** (inclusa la Casa piccola iniziale). Le grandi richiedono tuttavia un Forno e Pane; a **112 abitanti** una sola filiera Cibo da 1/s non è più nominalmente sufficiente.
- L'audit elenca anche le filiere alimentari e gli addetti necessari *a regime*, chiarisce quando un bonus a 2× non produce più benefici e propone confronti di gioco futuri controllati. È un **calcolo teorico**, non una dichiarazione di strategia vincente o una richiesta implicita di cambiare prezzi/bonus.
- Nessuna modifica alla GUI v25, al lettore PowerShell v26, al motore, al catalogo o ai checkpoint SQLite; il gate resta **177 test previsti su Windows** e gli output quantitativi sono ancora da acquisire.

**Raccomandazione, non decisione:** rinviare Carne finché non sia chiaro il vettovagliamento militare; non rendere demolizione obbligatoria per M4 salvo che i test mostrino errori irreversibili di progressione. Il rimborso del **25%** resta soltanto una proposta in [Q-08](scenarios/DEMOLITION_M4_REVIEW.md).

### M4.14 — Decisioni pronte per confronto (v28)

- La [scheda operativa D0](design/D0_DECISION_BOARD.md) separa otto domande e proposte da conferme dell'utente e collega ciascuna ai propri gate.
- Gli studi M5.1–M5.3 sono ora coerenti con la regola **attuale `civil-growth-5`**: l'eventuale `civil-growth-6` è soltanto un futuro nome candidato con scelta S1 approvata; l'alternativa S2 richiede un nuovo ID di scenario e import esplicito.
- Il **gate v28 resta da eseguire** (177 xUnit attesi, UI 8-bit, benchmark reali). Nessuna filiera, regola o migrazione M5 è stata implementata.

### M4.15 — Prevenire cicli e duplicazioni nel trasporto (v29)

- Nuovo [studio M5.4](design/M5_4_NAVAL_PREFEASIBILITY.md): separa Cantiere, acquisto della nave, flotta salvabile e missione futura. Confronta navi monouso, riutilizzabili e capacità astratta, lasciando costi, equipaggio e capienza all'approvazione.
- Il primo viaggio **non** può dipendere da Monete disponibili solo dopo una conquista: la Zecca M5.2 è una fonte candidata precoloniale. Comandi e ID nave devono essere atomici, senza consumi o prenotazioni duplicate.
- **Solo progettazione**, stesso codice eseguibile di v25; il gate di riferimento è ora **v29**, sempre 177 test xUnit attesi e controllo dei render.

## 5. Gate di design del gioco completo — Prima di M5

**Stato: dossier preparatorio D0 redatto, decisioni da affrontare insieme all'utente; nessuna scelta aggiuntiva è approvata.** La [matrice completa dei 43 riferimenti e dei 12 edifici esistenti](design/PRE_M5_DECISION_GATE.md) presenta opzioni, dipendenze e rischi. La [scheda v28](design/D0_DECISION_BOARD.md) ne organizza le otto approvazioni con i rispettivi gate e la scelta S1/S2 dei salvataggi. Questo gate anticipa le decisioni di prodotto, senza anticipare l'implementazione di M6.

1. **Catalogo finito:** fissare tutti i tipi di edificio previsti e separarli in obbligatori, alternativi, facoltativi e decorativi. I riferimenti originali suggeriscono **43 tipi strutturali**, ma non sono un elenco definitivo approvato. Gli edifici implementati sono 12.
2. **Condizione di vittoria (Q-01):** decidere se possedere contemporaneamente ogni tipo obbligatorio, averlo costruito almeno una volta o ottenere anche un traguardo finale. Definire cosa succede dopo demolizioni e cariche precedenti.
3. **Ruolo delle isole (Q-02):** scegliere quali esplorazioni/conquiste siano necessarie, quali risorse o edifici sblocchino e quali siano opzionali.
4. **Popolazione militare (Q-09):** reclutamento, addestramento, costi, mantenimento, eventuali perdite, congedo e rapporto con il bacino dei lavoratori.
5. **Tempo e difficoltà:** chiarire se il gioco completo resta a velocità 1:1 o offrirà accelerazione; stabilire effetti su punteggio e recuperabilità degli errori.
6. **Contratto dei contenuti:** definire una tabella per ogni nuovo edificio/risorsa: prerequisiti, costo, addetti, input/output, capacità, benefici, fase di sblocco e ragione strategica.

**Vincolo aritmetico aggiornato:** con ritmo iniziale di 60 s, limite superiore di **un arrivo ogni 30 s** e otto abitanti iniziali, in otto ore si possono avere **al massimo teorico 968 abitanti** soltanto assumendo fin dall'inizio case, Pane e Cibo sufficienti e accelerazione al tetto. È un limite ideale, non una strategia praticabile né una misura: i primi edifici e le scorte reali riducono il numero. M5 dovrà misurare il bilancio fra manodopera e reclute con le nuove regole.

**Criterio di uscita:** specifica approvata del catalogo, grafo delle dipendenze e della vittoria, più almeno una prima cronologia teoricamente raggiungibile. Non avviare grandi nuove filiere industriali prima di questo gate.

**Preparazione prima del codice M5:** confrontare la classificazione candidata delle 43 strutture, le tre opzioni di vittoria, il ruolo delle isole e il bilancio civile/militare nel [dossier D0](design/PRE_M5_DECISION_GATE.md). Gli studi [M5.1](design/M5_1_INDUSTRY_PREFEASIBILITY.md) e [M5.2](design/M5_2_FORGE_MINT_PREFEASIBILITY.md) rendono verificabili alcune alternative industriali, senza approvarle. Il gate M4.8 è riferito superato; i JSON reali restano utili per la valutazione finale di M4.

## 6. M5 — Economia avanzata, esercito e isole

**Stato: non iniziata.** Gli incrementi seguenti sono una proposta organizzativa; possono essere modificati dal gate di design.

| Fase | Contenuto | Verifica per chiuderla |
| --- | --- | --- |
| **M5.1** | Miniere, ferro, carbone, oro e lavorazioni primarie | Input e costi raggiungibili, contesa e capacità provate |
| **M5.2** | Forgia, Spade, Zecca e Monete; confronto fra produzione continua e per fasi | Filiere complete, concorrenza sul carbone e funzione economica/militare misurate |
| **M5.3** | Caserma, reclutamento e addestramento | Conteggi coerenti: civili, liberi, reclute e soldati; avanzamento/salvataggio |
| **M5.4** | Cantiere navale, navi e requisiti di trasporto | Costruzione, disponibilità e impiego dei mezzi verificati |
| **M5.5** | Esplorazione e spedizioni temporizzate | Stessa cronologia con passi diversi; esito applicato una sola volta |
| **M5.6** | Conquista di isole, ricompense, colonie/commercio se approvati | Sblocchi persistenti e nessun guadagno duplicato |
| **M5.7** | Castelli, strutture militari avanzate e scenario integrato | Dall'economia civile a una conquista completa con save/load |

Regole da seguire in M5:

- L'intero sistema militare utilizza il **tempo simulato**, non l'orologio reale.
- Le missioni saranno inizialmente **deterministiche e gestite per schede**, senza combattimento tattico o mappa di posizionamento, salvo nuova scelta.
- Soldati, navi e missioni sono stati distinti dagli edifici anche se condividono concetti come costo e prerequisito.
- I salvataggi militari richiederanno versionamento e una migrazione esplicita dal checkpoint civile esistente, senza conversioni silenziose.
- Le ricette alternative (per esempio Carne/Pesce) richiedono un modello di scelta preciso: il catalogo attuale usa **una ricetta per edificio** e la proiezione della filiera unica non copre produttori alternativi.
- Ogni fase include test Core/Application, simulazione della filiera, stato di lettura e verifica dei checkpoint; la GUI può crescere insieme alle funzionalità.

**Criterio di uscita M5:** una partita può avanzare da materiali e popolazione civili fino a un esercito equipaggiato, un viaggio, una conquista e la relativa ricompensa, essere sospesa durante una missione e riprendere senza duplicazioni.

### Preparazione tecnica di M5.1, senza anticipare il gate M4

Il [dossier M5.1](design/M5_1_INDUSTRY_PREFEASIBILITY.md) propone **M5.1a ferro/carbone**, **M5.1b oro** e **M5.1c confronto di scala/contesa**. Una base numerica candidata (non adottata) prevede il primo ferro da tre strutture per **450 legno lavorato / 320 pietra**, tre lavoratori e limite di **1 ferro/s**; una seconda variante con prezzi 1,5× consente di progettare un confronto. Sono esplicitati deposito, prerequisiti, capacità di carbone condivisa, test di bilancio e due possibili politiche per il passaggio dei salvataggi M4 → M5.

**Fermo di progettazione:** i prezzi, le ricette e la strategia di migrazione non sono approvati; il codice M5.1 non deve partire prima del gate decisionale **D0** e della chiusura formale di M4. Il gate tecnico M4.8 è già riferito superato dall'utente, ma il solo esito non approva automaticamente i contenuti industriali.

### Preparazione di M5.2: Forgia, Zecca e carbone condiviso

Il [dossier M5.2](design/M5_2_FORGE_MINT_PREFEASIBILITY.md) propone, **solo per un confronto futuro**, Forgia (240/180, 1 Ferro + 1 Carbone → 2 Spade/s) e Zecca (210/160, 1 Oro + 1 Carbone → 3 Monete/s), con un addetto ciascuna. Assumendo anche i valori A di M5.1, le **due fonderie e i due trasformatori** richiederebbero **4 Carbone/s**: una miniera da 2/s non li mantiene tutti attivi contemporaneamente. Sono individuate due possibili strategie: **produzione a fasi con una miniera**, oppure **seconda miniera per sovrapporre le filiere**. Costi cumulativi teorici: **1.300/950** per l'insieme con una miniera, **1.430/1.040** con due. Non sono valori adottati né tempi misurati.

Occorre definire anche il *significato* di Spade e Monete, inclusa la possibile moneta ottenuta dai commerci coloniali: una risorsa consumabile per specifici comandi non equivale a un sistema monetario applicato a tutti gli edifici. Questo punto entra nel gate D0, perché influisce sul reclutamento, sulle navi e sui salvataggi. **M5.2 non entra nel codice** prima di M5.1 verificata e delle decisioni sul modello economico.

### Preparazione di M5.3: Caserma e popolazione condivisa

Il [dossier M5.3](design/M5_3_ARMY_PREFEASIBILITY.md) porta il requisito *Spade + abitanti → Caserma → Soldati* in un contratto verificabile, senza fissare valori non presenti nei riferimenti. L'invariante proposto distingue **civili impiegati, civili liberi, reclute e soldati**, senza duplicare persone. Confronta arruolamento immediato, coda temporizzata e capacità per Caserma; propone M5.3a arruolamento atomico, M5.3b addestramento e persistenza e M5.3c confronto dell'impatto civile. Restano **da approvare D0.4/D0.8**: consumo o dotazione delle Spade, tempi, cancellazione/congedo, razioni, capienza e perdite. Nessun soldato è stato aggiunto all'attuale `civil-growth-5`.

### Preparazione di M5.4: Cantiere navale e flotta

Il [dossier M5.4](design/M5_4_NAVAL_PREFEASIBILITY.md) confronta nave monouso, entità persistente e capacità astratta, senza adottare un modello. Propone **M5.4a** primo Cantiere/acquisto atomico, **M5.4b** flotta con ID e salvataggi, **M5.4c** preflight di trasporto senza far partire missioni. Tavole e Monete compaiono nel riferimento, **non** i loro prezzi, le capienze, la durata della costruzione o la necessità di equipaggio. M5.5 si occuperà di spedizioni, M5.6 di conquiste/colonie. Servono D0, S1/S2, economia e Caserma approvate e validate prima di scrivere codice.

## 7. M6 — Vittoria, risultati e bilanciamento complessivo

La **regola** della vittoria viene approvata nel gate di design; in M6 la rendiamo completa, verificabile e bilanciata.

| Fase | Lavoro | Evidenza attesa |
| --- | --- | --- |
| **M6.1** | Verifica degli obiettivi finali e dell'eventuale costruzione conclusiva | Vittoria rilevata una sola volta; condizioni negative testate |
| **M6.2** | Strategia automatica dall'inizio alla vittoria | Cronologia riproducibile e raggiungibilità provata |
| **M6.3** | Durata della strategia efficiente | Tempo effettivamente misurato rispetto all'obiettivo di circa **8 ore** |
| **M6.4** | Strategie meno efficaci ma realistiche | Confronto fino a **32 ore o più**, senza attese artificiali obbligatorie |
| **M6.5** | Bilanciamento di prezzi, produttività, personale, sblocchi e recuperi | Report per versione delle regole, nessuna filiera inutilmente dominante |
| **M6.6** | Risultati, cronologia e classifica locale | Identità delle regole e tempo di gioco registrati senza ambiguità |
| **M6.7** | Prove con giocatori, comprensibilità e ritmo | Problemi osservati e revisioni documentate |

**Non assumere** che otto ore garantiscano divertimento o che un benchmark dimostri l'ottimalità. La classifica pubblica resta un'opzione separata da decidere con Q-10; non anticipare server e account per una classifica locale.

## 8. Traccia parallela — GUI pixel art 8-bit

La base retro monospace è **implementata, verificata e approvata**. Il layout corrente include menu laterale da 240 px, card comuni, icona abitanti, costi e produzione con icone e hover ocra. Il [contratto grafico](PIXEL_ART_UI.md) distingue questi elementi dalle rifiniture future.

| Quando | Incremento grafico | Gate |
| --- | --- | --- |
| M4 e prima industria — completate | Prototipo, monospace, icone civili/industriali, menu, card e dettaglio compatti | Verificati 1040×740 e 1240×850; DPI fisici da ampliare |
| Durante M5 — prossime fasi | Aggiungere icone e sezioni per esercito, navi e isole | Ogni comando essenziale raggiungibile; nessuna formula duplicata |
| Durante M6 | Migliorare priorità visiva di costi, produzione, missioni e obiettivi | Prove di comprensibilità con scenari complessi |
| M7 | Rifinitura estetica e accessibilità | Sprite nitidi, contrasto, tastiera, ridimensionamento |

Asset **originali o con licenza compatibile**. La direzione è descritta in [PIXEL_ART_UI.md](PIXEL_ART_UI.md) e nell'[ADR 0012](adr/0012-retro-8bit-desktop-visual-language.md). Non programmare la GUI finale tutta in M7.

## 9. M7 — Consolidamento, qualità e distribuzione

1. Correzione dei bug e test di regressione, senza nuova espansione significativa delle regole.
2. CI per build e test, audit delle dipendenze quando disponibile, controllo dei pacchetti effettivamente distribuiti.
3. Prove delle migrazioni, errori SQLite, rollback e preservazione dei salvataggi personali.
4. Profilazione con molte strutture, partite lunghe e più missioni in corso.
5. UI pixel art definitiva e verifica di accessibilità, DPI, tastiera e leggibilità.
6. Pacchetto Windows, avvio nativo, installazione e aggiornamento; altre piattaforme solo se testate.
7. Manuale per i giocatori, note di rilascio e prima versione pubblicabile.

**Criterio di uscita:** una partita completa e salvabile supera le verifiche dichiarate, le piattaforme supportate sono esplicitate e il prodotto è distribuibile.

## 10. Decisioni aperte e loro ordine

| Priorità | Tema | Stato | Prima di |
| --- | --- | --- | --- |
| Alta | Catalogo della prima campagna, fra i 43 tipi di riferimento | **Scelti 22 tipi; altri 21 fuori**. Contratto di vittoria distinto | Gate di design M5 |
| Alta | Vittoria Q-01 e conquiste Q-02 | **Scelte:** possesso dei 22 tipi, Castello dopo una conquista obbligatoria; parametri missione aperti | Implementazioni M5.5–M6 |
| Alta | Reclutamento, perdite, mantenimento Q-09 | **Da decidere** | M5.3 |
| Alta | Ruolo di Spade/Monete, impiego nei comandi e fonte coloniale alternativa | **Da decidere** | M5.2 / M5.3 / M5.4 |
| Media | Carne: filiera civile alternativa o vettovagliamento militare | **Fuori dalla prima campagna per D0.1**; eventuale ruolo futuro da decidere | Nuova ricetta/missione che la usi |
| Media | Demolizione Q-08; 25% rimborso solo candidato | **Rinviata dall'utente** | Eventuale incremento successivo |
| Media | Tempo 1:1 oppure accelerazione e impatto sulla classifica | **Da decidere per il gioco completo** | M6.6 |
| Successiva | Classifica locale o pubblica Q-10 | **Da decidere** | Eventuale servizio pubblico |

Le decisioni già adottate per M2–M4 non vengono annullate da questa lista. La tabella documenta **estensioni ancora aperte**.

## 11. Regola di avanzamento e resoconto fisso

Ogni incremento futuro deve comunicare sempre questi cinque elementi, anche quando l'utente non può testare:

1. **Dove siamo:** milestone, versione del pacchetto, ultima validazione effettivamente confermata.
2. **Cosa abbiamo fatto:** codice/documenti cambiati, decisioni adottate, risultati misurati.
3. **Cosa stiamo implementando:** obiettivo limitato dell'incremento, file/moduli interessati, impatto su regole e salvataggi.
4. **Cosa ci aspetta:** prossimo passo e dipendenze verso la chiusura della milestone e la roadmap futura.
5. **Cosa rimane aperto:** test non eseguiti, rischi, scelta dell'utente richiesta, limite noto e criterio di uscita.

Lo stato deve usare esplicitamente **Proposto / Implementato non validato / Verificato / Rinviato**. Una conferma dell'utente sui test è un gate riferito dall'utente; il report numerico locale non va inventato. I documenti da allineare sono [game design](GAME_DESIGN.md), [handoff](HANDOFF.md), [changelog](../CHANGELOG.md) e gli ADR pertinenti. Le regole nuove richiedono una scelta dell'utente; un piano o uno studio non autorizza ad applicarle.

**Prossima sequenza:** contesa/scala M5.1c → contratti Spade/Monete e M5.2 Forgia/Zecca → regole di reclutamento e M5.3 Caserma → parametri navali e M5.4 Cantiere/flotta → M5.5–M5.7 missioni, conquista obbligatoria e Castello → M6 vittoria/classifica. M4, catalogo/vittoria/ruolo della conquista/S1 e parametri A di M5.1a/b sono acquisiti; non richiedono nuova conferma.
