# Handoff — archivio delle sessioni precedenti

Archivio conservato durante il riordino documentale del **2026-10-27**. Per lo stato corrente usare [HANDOFF.md](HANDOFF.md). I passaggi seguenti riportano anche decisioni allora aperte e verifiche allora pendenti; non sostituiscono il punto di ripresa attuale.

## Punto di ripresa v40 — M5.2c verificata (2026-11-02)

Aggiornato il **2026-11-02**, checkpoint **v40 / M5.2c Verificato — M5.2 chiusa**. [Report corrente](validation/M5_2C_FORGE_MINT.md), [storico](HANDOFF_HISTORY.md), [roadmap](ROADMAP.md).

### Stato verificato

M0–M4 completate; **M5.1 e M5.2 Verificate**. Regole **`civil-growth-9`**, **19 tipi di edifici / 16 risorse**. Restano Caserma, Cantiere e Castello per i 22 tipi della prima campagna. Il prossimo incremento è il contratto M5.3 Caserma/reclutamento: mancano scelte militari, non le produzioni di Spade/Monete.

Gate completo `validate.cmd`: **260/260 test**, Release **zero avvisi/errori**, raggiungibilità dei due cataloghi, **23 cronologie più crescita accelerata**, **sei benchmark**, pubblicazione e smoke nativi Windows dei due scenari. Run **`1c74597068ab4f96b4f6fd6d30518b45`**. TRX e **33 PNG** (31 richiesti più due dettagli); nessuna nuova modifica UI in v40. Il sesto benchmark e le tre cronologie `crafting` sono conservati anche nella cartella della run, con percorsi in `validation.json`.

| Suite | Superati |
| --- | ---: |
| Core | 67 |
| Application | 97 |
| Infrastructure | 35 |
| Desktop | 61 |
| **Totale** | **260** |

### M5.2c: misure e limiti

- Nuovo `--benchmark-forge-mint` sul **catalogo corrente**, senza cambiare prezzi, ricette o regole: **nove profili e quattro guardie**, ogni caso a passi interi e 250+750 ms, con ripristino di un checkpoint frazionario e confronto finale di metriche/snapshot.
- Obiettivo controllato **240 Spade /360 Monete**: **241 s** a fasi con una miniera, **122 s** paralleli con due. Acquisti industriali **1.300/950** contro **1.430/1.040** Legno lavorato/Pietra. Con estrazione attiva, picco industriale **5** contro **8** addetti, esclusi i sette addetti alimentari preesistenti.
- Colonia sintetica comune 40/40, due Magazzini, Cibo/Pane attivi; le infrastrutture civili sono preesistenti ed escluse dai costi. Controlli con minerali precaricati; prove sostenute da **1.800 s e otto ore** con miniere minerali attive da scorte zero, contabilità di tutte le risorse a ogni secondo e nessuna carenza alimentare.
- A 30 minuti: fasi **1.680 Spade /1.998 Monete**, parallelo **2.000/1.998**, una miniera con tutto attivo **0/0**. Nel terzo caso le prime produzioni arrivano a **2.002 s** dopo il riempimento dei depositi dei metalli; obiettivo a **2.240 s**. A otto ore tutti i profili raggiungono **2.000/1.998**, senza lotti parziali.
- Nuova strategia **`crafting` dalla nuova partita**: Oro a 1.500 s; seconda miniera Carbone, Forgia e Zecca a **2.200 s**, acquisti aggiuntivi **580/430**. Primo lotto a **2.201 s**; **240 Spade e 360 Monete a 2.320 s**, popolazione 48. A otto ore **1.000/999** alla capienza base. Cronologia fattibile, non ottima né misura della durata della campagna completa.
- Test aggiunti: replay integrato da checkpoint a 2.200,5 s; SQLite con Forgia/Zecca attive, prodotti e residuo di mezzo secondo. Il Carbone rimasto viene assegnato prima alla Forgia anche dopo il caricamento, senza spese o prodotti duplicati.

Artefatti: `.local-cache/m4/validation.json`, `crafting-validation.log`; nella run `forge-mint-benchmark.json`, `crafting-start.json`, `crafting-target.json`, `crafting-8-hours.json`. Una copia corrente è anche sotto `.local-cache/m4/`. L'esperimento A/B storico resta separato su v7; B non entra nel catalogo giocabile e non è stato rieseguito in v40.

### Regole, salvataggi e UI acquisiti

- Forgia: **240/180**, 1 addetto, Fonderia di ferro posseduta, **1 Ferro +1 Carbone → 2 Spade/s**, priorità 100.
- Zecca: **210/160**, 1 addetto, Fonderia d'oro posseduta, **1 Oro +1 Carbone → 3 Monete/s**, priorità 101, dopo Ferro 90 e Oro 91.
- Spade e Monete iniziali zero, capacità **1.000 +500 per Magazzino**, lotti interi. Spade consumate dal futuro arruolamento; Monete per pagamenti espliciti, non valuta generale.
- S1, stesso slot e SQLite/snapshot **v2**; conversioni v1–v8→v9 con backup originale alla prima scrittura. Nuovi contenuti a zero, scorte, lavoratori, persone, tempo e frazioni preservati. Nessun cambio di regole/formato in v40.
- Cataloghi storici v4–v8 preservati. UI retro, ordinamento italiano, saldi e stima riempimento; effetti v38 nello stesso elenco con icone e monospace 12 px di costi/produzione/consumi.
- Campagna da 22 tipi, vittoria col possesso di tutti e Castello conclusivo dopo conquista obbligatoria; grafica e ruolo della conquista già scelti. Carne fuori e demolizione **Rinviata**. Non richiedere nuovamente queste scelte.

### Prossimo lavoro e decisioni aperte

1. **M5.3, prima del codice militare:** aggiornato il [dossier Caserma](design/M5_3_ARMY_PREFEASIBILITY.md). Scegliere prezzo/prerequisiti Caserma, numero di Spade per recluta, modello di addestramento (immediato/coda/posti simultanei), durata e capacità, alloggio/razioni, cancellazione/congedo. Il consumo delle Spade all'arruolamento è già deciso; non riaprirlo.
2. Dopo il contratto, M5.3a comando atomico e popolazione condivisa, M5.3b addestramento e salvataggi, M5.3c impatto civile. Non introdurre valori plausibili per deduzione. Perdite e missioni richiedono contratti propri, senza anticipare navi o combattimenti.
3. Poi M5.4 flotta, M5.5–M5.7 missione/conquista/Castello e M6 vittoria. Le circa 8 ore efficienti /32+ ore lente restano obiettivi del gioco completo.

Restano aperti DPI fisici, altre piattaforme e accessibilità. [Guida di sviluppo](DEVELOPMENT.md).

Ultimo aggiornamento: 2026-10-27. Questo documento descrive il punto operativo da cui continuare; visione e regole complete sono nel [game design](GAME_DESIGN.md).

## Punto di ripresa corrente — M4 chiusa, M5.1b verificata

**Decisioni ricevute:** prototipo monospace approvato, demolizione rinviata; D0.2 possesso dei 22 tipi con Castello conclusivo; D0.3 una conquista obbligatoria sblocca il Castello; continuità S1 con conversione/backup; configurazioni **A per ferro/carbone e A per Oro approvate con risposte distinte**. Ferro prioritario quando il carbone scarseggia. Non richiedere nuovamente queste scelte.

**Implementato e verificato:** `civil-growth-7`, **17 edifici / 14 risorse**. Oro: miniera 180/120, output 1 minerale/s; fonderia 220/170, 1 minerale + 1 carbone → 1 oro/s, un addetto ciascuna. Ferro invariato. Capacità nuove 1.000 +500 per Magazzino, scorte iniziali zero. Conversione da `-1…-6` con SQLite v2 e backup; ferro esistente, personale, Magazzini e frazioni preservati. Cataloghi v4/v5/v6 disponibili; UI con quattro ulteriori icone.

**Ultimo intervento UI richiesto:** dettaglio con descrizione → costi in elenco → produzione/effetti ed eventuali consumi; costi aggiornati per +1/+5/+10. Sidebar da 240 px con menu Edifici/Risorse; card abitanti/posti/liberi con icona pixel originale, comune alle due pagine. Costi e produzione condividono icone risorsa da 14 px e testo normale da 12 px. Colonne numeriche Risorse compattate per conservare la leggibilità dei nomi nel layout minimo. Conservati monospace e comandi/diagnosi fissi. Vedere [direzione grafica](PIXEL_ART_UI.md).

**Ultimo ritocco del tema:** tutti i pulsanti abilitati in hover usano l'ocra `#E6D5AC` della struttura selezionata e testo scuro, tramite la risorsa condivisa `SelectionBackground`. Hover applicato anche al presenter Fluent; disabilitati grigi e geometria stabile. Test esistenti estesi a Costruisci e Salva.

**Ultimo gate effettivo:** `.\validate.cmd` codice 0, **206/206 test** (67 Core, 61 Application, 30 Infrastructure, 48 Desktop), zero avvisi/errori, 16 cronologie e prova accelerata, quattro benchmark, 17 render standard più due render del nuovo dettaglio, smoke nativi di entrambi gli scenari. Run `899f6ff3e8ff41288330c3d26b4c4ea7`; verificati anche hover ocra, testo scuro e disabilitazione con puntatore fermo. Primo oro da nuova partita a **1.502 s**, con 401 ferro e 402 carbone; Oro/Ferro 1.000 e popolazione 48 a otto ore. Contesa carbone e ripresa dopo seconda miniera verificate.

**Prossimo incremento:** M5.1c scala e contesa del carbone, poi parametri/contratti Spade e Monete prima di M5.2. Restano cinque tipi futuri del catalogo da 22 e l'implementazione di militari, navi, conquista e vittoria. [Report corrente Oro](validation/M5_1B_GOLD.md), [report ferro precedente](validation/M5_1A_IRON.md) e [scheda D0](design/D0_DECISION_BOARD.md). Le sezioni seguenti conservano gli stati precedenti.

## Ultimo intervento — tipografia retro monospace

Su richiesta dell'utente, applicato **Consolas** a tutta l'interfaccia, con fallback DejaVu Sans Mono, Liberation Mono e Courier New. Unica risorsa `ContentControlThemeFontFamily` in `App.axaml`, riusata dalla finestra e dal tema Fluent. Nessun nuovo pacchetto o font distribuito; regole e salvataggi invariati.

**Verificato:** `.\validate.cmd` completato con codice 0, **177/177 test**, zero avvisi/errori, simulazioni e prove native di entrambi gli scenari superate. Run `f87ccb76a0c245bd95c4db8589d0a194`; render civili ispezionati a 1040×740 e 1240×850, compresi Risorse e stato avanzato del Pane in formato compatto. Le decisioni D0 già registrate e quelle pendenti restano distinte da questa modifica grafica.

## Ultimo incremento — esperimento industriale A/B eseguito

Creato ed eseguito [IndustryProbe](../experiments/industry-probe/README.md), eseguibile opzionale fuori dalla soluzione di produzione: quattro combinazioni A/B × ferro/ferro+oro sotto due politiche di acquisto. Ogni confronto riconcilia 3.601 secondi e viene ripetuto con passi da 1 s e 250+750 ms, con rapporti e snapshot finali identici. Saturazione e ripresa dopo un Magazzino pagato verificate. Build della prova senza avvisi/errori; nessun pacchetto esterno nuovo.

Con acquisto progressivo, primo ferro A/B **452/677 s**, primo oro **852/1.277 s**. Il Magazzino intermedio B richiesto dalla politica di budget intero è evitabile comprando progressivamente. Artifact locale: `.local-cache/industry-probe/runs/a419e939ab024a5fa6573135d31dedff/comparison.json`. La colonia iniziale è sintetica, con 40 abitanti e filiere civili già pronte: prezzi, ricette e migrazioni non sono adottati nel gioco.

**Nuova validazione completa:** `.\validate.cmd` terminata con codice 0, **177/177 test**, zero avvisi/errori, simulazioni e prove native di entrambi gli scenari superate; TRX e render in `.local-cache/m4/runs/4c51d35a1c924fdca6656687e8f21e0e/`. Il benchmark opzionale ha i propri controlli e non aumenta il conteggio xUnit. D0.1 è scelta; vittoria/isole, altri contratti e S1/S2 restano da decidere prima dell'integrazione M5.

## Ultima verifica effettiva — correzione del gate Windows

**Verificato in questa sessione su Windows:** `.\validate.cmd` termina con codice 0 dopo la correzione dei pennelli statici di `PixelGlyph`. La palette usa `ImmutableSolidColorBrush` e `IImmutableBrush`, evitando l'accesso a pennelli legati a un altro thread quando il tipo viene inizializzato dal controllo del catalogo. Prima della correzione fallivano 21 test Desktop; dopo, **177/177 test superati** (Core 67, Application 46, Infrastructure 26, Desktop 38), build Release senza avvisi/errori, cataloghi, simulazioni, quattro benchmark e prove native di entrambi gli scenari completati. Nove PNG generati; la valutazione estetica del prototipo D0.7 resta aperta.

Evidenza locale: `.local-cache/m4/validation.json` e `.local-cache/m4/runs/98d961210607483a8afe8d708039efab/` (TRX, render e log nativi). Questo esito aggiorna i riferimenti al gate Windows pendente riportati sotto e negli altri documenti precedenti. Regole `civil-growth-5` e formato dei salvataggi invariati. Prossimo passo M4: analizzare i benchmark e valutare il prototipo, poi discutere le decisioni D0 già elencate nella roadmap.

## Lettura dei risultati completata

Eseguito `scripts/report-m4.ps1` e verificata la stessa uscita sui cinque JSON conservati in `runs/98d961210607483a8afe8d708039efab/benchmark-inputs/`, con impronte SHA-256. Ispezionati tutti i nove render. Risultati e limiti nel [rapporto misurato M4](validation/M4_MEASURED_RESULTS.md): 136 abitanti in 4.406 s per il profilo accelerato con sole case piccole; a 220 abitanti la snella e la completa arrivano entrambe a 250 senza carenze, mentre a 320 la snella arriva a 347 con 126 s senza Pane e la completa a 350 senza carenze.

**Prossimo lavoro:** valutazione utente del prototipo D0.7 e classificazione della demolizione per chiudere M4. La scelta D0.1 sotto registrata pone Carne/Cacciatore/Allevamento fuori dalla prima campagna. Si possono discutere in parallelo vittoria e isole, senza anticiparne il codice.

## D0.1 — Catalogo della prima campagna scelto dall'utente

Il 2026-10-27 l'utente ha scelto «Prima campagna con i 22 tipi proposti», con domanda limitata al catalogo. Elenco finito, dieci ID futuri candidati, classificazione di tutti i 43 riferimenti, proposta di vittoria e sette casi di accettazione nella [scheda prima campagna](design/FIRST_CAMPAIGN_PROPOSAL.md). D0.1 è approvata; D0.2/D0.3, costi, ricette, contratti militari/navali e S1/S2 restano aperti. Nessun cambiamento a Core, GUI, regole o SQLite.

## Stato pianificato prima della verifica Windows (riferimento storico)

- M0–M3 sono completate. L'utente ha confermato **«test passati» fino al checkpoint v24/M4.10** (174 casi xUnit attesi; nessun TRX/JSON allegato), che introduce case acceleranti (`civil-growth-5`), frazione persistita e confronti storici. **v25/M4.11** avvia la prima interfaccia 8-bit originale, con icone per tutti i 12 edifici e le 9 risorse; **v26/M4.12** aggiunge un lettore dei quattro benchmark M4; **v27/M4.13** introduce l'[audit teorico degli incentivi abitativi](scenarios/HOUSING_INCENTIVE_AUDIT.md); **v28/M4.14** aggiunge la [scheda D0](design/D0_DECISION_BOARD.md) e corregge gli studi M5; **v29/M4.15** prepara la [prefattibilità M5.4 navale](design/M5_4_NAVAL_PREFEASIBILITY.md). v26–v29 **non modificano il gameplay o la GUI**. **177 casi xUnit attesi e nuovo gate Windows da eseguire** (SDK .NET e PowerShell assenti qui). M4 non è formalmente conclusa: servono il nuovo gate, i numeri grezzi dei benchmark per analisi quantitative e la classificazione di Carne/demolizione. Gli studi [D0](design/PRE_M5_DECISION_GATE.md), [M5.1](design/M5_1_INDUSTRY_PREFEASIBILITY.md), [M5.2](design/M5_2_FORGE_MINT_PREFEASIBILITY.md), [M5.3](design/M5_3_ARMY_PREFEASIBILITY.md) e [M5.4](design/M5_4_NAVAL_PREFEASIBILITY.md) **non approvano nuovi contenuti**.

**Piano di riferimento:** [ROADMAP.md](ROADMAP.md). Sequenza: **gate v29** (stesso desktop di v25, build, 177 test, validatore, smoke, render 8-bit ed ergonomia a 1040×740 e 1240×850) → esecuzione opzionale del [lettore M4](validation/M4_BENCHMARK_READER.md) sui quattro JSON generati → confronto con l'[audit delle case](scenarios/HOUSING_INCENTIVE_AUDIT.md), revisione quantitativa e chiusura delle scelte M4 → **decisioni nella scheda D0**, incluso giudizio sul prototipo D0.7 e catalogo/vittoria/isole/militari/armi-monete → scelta di salvataggio **S1/S2** e raggiungibilità D0.6 → M5.1 ferro/carbone/oro → M5.2 Forgia/Zecca → M5.3 reclutamento → M5.4 Cantiere/flotta/preflight → M5.5–M5.7 missioni/isole/progressione. Interfaccia per schede, senza collocazione sulla mappa. I dossier M5 restano proposte.

| Oggetto | Stato effettivo | Prossima prova/decisione |
| --- | --- | --- |
| M0–M3 | Completate, checkpoint documentati | Evitare regressioni |
| M4 fino al benchmark di scala Pane | **Test e validazione superati secondo l'utente** (162 xUnit attesi; output grezzi non disponibili) | Recuperare report se disponibili, senza riaprire il gate per mancanza di allegati |
| Proiezione deficit Forno e capacità della filiera | **Gate v17 riferito superato dall'utente** (163 xUnit attesi; report grezzi assenti) | Preservare le verifiche e acquisire eventuali TRX/JSON |
| Benchmark filiera snella a 220 residenti | **Gate v17 riferito superato dall'utente** (quattro strategie) | Analizzare il JSON quando disponibile; non inventare valori finali |
| Stress oltre soglia a 320 residenti | **Gate v21 riferito superato dall'utente; output non disponibili** | Leggere i JSON di 220/320 abitanti prima di concludere su tempi e carenze |
| Ergonomia GUI provvisoria v22 | **Gate riferito superato dall'utente (166 casi attesi)** | Conservare prerequisiti sempre visibili e colori distinti |
| Correzione salto hover v23 | **Gate riferito superato dall'utente** | Preservare bordo a 1 pixel e geometria invariata del costo/selettore/pulsante |
| Crescita accelerata v24 / M4.10 | **Gate riferito superato dall'utente** | +15% per Casa piccola oltre la prima, +30% per Casa grande, cap 2×; 174 test attesi, backup v4→v5, benchmark nuovo contro storico |
| Primo prototipo pixel art v25 / M4.11 | **Implementato, nuovo gate pendente** | 21 pittogrammi 8×8, bordi netti, stessi dati e comandi; 177 test attesi e controllo visivo render |
| Lettore quantitativo M4 v26 / M4.12 | **Script predisposto, non eseguito** | `scripts/report-m4.ps1` confronta abitazioni v4/v5, Pane 220/320 e report validation; risultato numerico non disponibile senza JSON |
| Incentivi case M4 v27 / M4.13 | **Audit aritmetico redatto, gameplay invariato** | Quattro combinazioni con costo abitativo 560/280 e capienze 64/80/96/112; confrontare con i benchmark reali dopo il gate |
| Scheda decisionale D0 v28 / M4.14 | **Preparata, nessuna decisione utente adottata** | Otto questioni e criteri di gate; `civil-growth-5` attuale e `-6` soltanto futuro candidato S1 |
| Flotta e Cantiere M5.4 v29 / M4.15 | **Studio preparato, nessun codice navale** | Decidere nave monouso/riutilizzabile, costi di Cantiere e nave, trasporto, prima nave, save e navi commerciali |
| Gate D0 prima di M5 | **Dossier preparato in v19, decisioni non approvate** | Classificare i 43 riferimenti, definire vittoria/isole/militari e verificare la raggiungibilità |
| Prime filiere industriali M5.1 | **Prefattibilità preparata in v20, nessun codice M5** | Valutare costi A/B, capacità, carbone condiviso e migrazione S1/S2 solo dopo i gate M4/D0 |
| Forgia, Zecca, Spade e Monete M5.2 | **Prefattibilità preparata in v21, nessun codice M5** | Confrontare produzione a fasi/continua e approvare significato delle Monete e delle Spade prima di M5.2 |
| Caserma, reclute e soldati M5.3 | **Prefattibilità preparata in v23, nessun codice M5** | Decidere D0.4/D0.8, conservare popolazione e tempo, poi implementare solo dopo M5.1/M5.2 |
| M5–M7 e contenuti definitivi | **Pianificati, non implementati** | Prima approvare catalogo, vittoria e vincoli militari/isole |

Ogni prossimo resoconto seguirà i **cinque campi obbligatori** definiti nelle [convenzioni di sviluppo](DEVELOPMENT.md): dove siamo, fatto, in corso, prossimi passi, questioni aperte/verifiche. Un test previsto non è un test eseguito.

**Incremento da riprendere per primo:** gate v29/M4.15 su Windows, contenente lo stesso codice eseguibile di v25/M4.11 e il lettore v26/M4.12. Non cambiare `civil-growth-5`, checkpoint SQLite o benchmark prima del gate. Verificare il controllo `src/SimRisorse.Desktop/Presentation/PixelGlyph.cs`, la sua integrazione in `Views/MainWindow.axaml`, e i tre casi aggiunti a `DesktopTests.cs`. Al termine della validazione controllare i PNG a entrambe le dimensioni e, con i quattro JSON, eseguire `scripts/report-m4.ps1` per ottenere tempi e carenze reali; confrontarli poi con i soli **limiti teorici**, non con prestazioni garantite, di M4.13. Ogni modifica estetica mantiene fermo il bordo di Costruisci e la diagnosi sopra il comando. Dopodiché discutere la [scheda D0](design/D0_DECISION_BOARD.md) e le scelte navali [N-01…N-07](design/M5_4_NAVAL_PREFEASIBILITY.md) senza trasformare preferenze in approvazioni automatiche.

## Cronologia degli incrementi precedenti
- 2026-10-27: magazzino (+500 per risorsa, 100 legno lavorato + 60 pietra, senza personale) e regole `civil-growth-2`, con conversione dei checkpoint `civil-growth-1` e copia di sicurezza del file sostituito. Valori e politica di compatibilità scelti dall'utente; decisione nell'[ADR 0010](adr/0010-storage-capacity-and-rules-conversion.md).
- 2026-10-27: filiera fattoria, pozzo, mulino e forno nelle regole storiche `civil-growth-3`, conversioni concatenate da `-1` e `-2`. L'output del forno era provvisoriamente Cibo; questa politica è stata sostituita in `-4` su decisione dell'utente.
- 2026-10-27: analisi statica di raggiungibilità del catalogo (`CatalogReachability`, `--check-catalog`), eseguita da test e validazione. Entrambi gli scenari risultano completi.
- 2026-10-27: diagnostica delle produzioni limitate, con nomi precisi di input insufficienti e output senza spazio, dettagli nella UI e tre nuovi test. **Al momento di questo incremento** la modifica non era stata eseguita nell'ambiente privo di SDK .NET: il checkpoint di allora era di 140 test e la suite proposta saliva a 143. Si tratta di cronologia storica, **non** del gate attuale (163 casi attesi). Nessuna variazione ai salvataggi o alle regole.
- 2026-10-27: approvata e implementata la [progressione quantitativa per case avanzate e Pane](scenarios/ADVANCED_HOUSING_PROPOSAL.md) in `civil-growth-4`: Casa grande +32 posti a costo 160 legno lavorato / 80 pietra, prerequisito Forno, senza personale; forno 3 Pane/s al posto di 3 Cibo/s; 0,015 Pane/s per residente oltre i posti delle Case piccole, più 0,010 Cibo/s per tutti. Capacità Pane 100 (+500 per Magazzino). Il pareggio aritmetico nei materiali è dalla quarta Casa grande; misurazioni con esecuzione .NET ancora in attesa.
- 2026-10-27: aggiunto [confronto fra tre strategie abitative](scenarios/HOUSING_BENCHMARK.md), richiamabile con `--benchmark-housing` e incluso nel validatore. Misura tempi per 48/100/136 abitanti, materiali, carenze e inattività del Forno in otto ore. **Predisposto e documentato, non eseguito con .NET**; non cambia contenuti o snapshot.
- 2026-10-27: aggiunti tre test di riproducibilità della progressione avanzata, inclusi intervalli temporali disomogenei e salvataggio SQLite a 879,5 secondi. Ripresa da pausa e confronto esatto con una partita continua fino a due ore; **test scritti ma non eseguiti**. Nessuna modifica di regole o persistenza.
- 2026-10-27: rafforzato il benchmark con riconciliazione di **tutti i 28.800 secondi** per strategia fra previsioni, scorte e crescita. Un nuovo test Core valuta il limite superiore a 200 residenti avanzati con uno o due Forni e potenziamento al minuto 15. [Valutazione](scenarios/HOUSING_BENCHMARK.md). **Gate .NET ancora in attesa**.
- 2026-10-27: diagnostica di Pane nella barra Crescita: confronta capacità nominale dei Forni e consumo avanzato, spiega input mancanti o scorte quasi piene e indica l'autonomia senza produzione al consumo attuale. Test Desktop aggiunto per evitare che il Forno fermo a deposito pieno sia segnalato come capacità insufficiente. [Regole di lettura](scenarios/CIVIL_GROWTH.md). **Render e test non ancora eseguiti**.
- 2026-10-27: corretto un vuoto della diagnosi **prima** del primo residente avanzato: con Casa grande pronta e Pane mancante, un messaggio guida la costruzione/assegnazione anche se gli abitanti avanzati sono ancora zero. Due test nuovi seguono la ripresa a 59 secondi validi da assenza di Pane (Desktop) e da assenza di Cibo con Pane ancora disponibile (Application); nessuna risorsa viene usata nello stesso secondo in cui è prodotta.
- 2026-10-27: riesaminata la [filiera della Carne](scenarios/MEAT_DESIGN_REVIEW.md) dai riferimenti originali. Cacciatore e Allevamento sono distinti, ma aggiungere una fonte di Cibo identica al Pesce rischia di non offrire scelte. Raccomandato valutare un uso per M5, senza considerarlo approvato; nessun contenuto implementato.
- 2026-10-27: l'utente ha scelto esplicitamente **GUI pixel art retro 8-bit essenziale** per il futuro desktop. La scelta è registrata nell'[ADR 0012](adr/0012-retro-8bit-desktop-visual-language.md) e nella [direzione grafica](PIXEL_ART_UI.md); icone, font e dimensioni restano da prototipare. Nessuna modifica alla GUI attuale o al motore.
- 2026-10-27: predisposto lo [studio M4 per demolizione](scenarios/DEMOLITION_M4_REVIEW.md): confronto di rimborso 0/25/50%, controlli atomici su capacità abitativa, magazzini, restituzione lavoratori e prerequisiti. **Rimborso 25% solo raccomandato, Q-08 non approvata e non implementata**; nessuna nuova versione delle regole.
- 2026-10-27: estratta la diagnostica della sostenibilità del Pane in `Application/ReadModels/BreadSupplyProjector`: `GameView.BreadSupply` espone uno stato enum e autonomia della scorta, utilizzabili anche dalla futura UI 8-bit. Il caso **Cibo e Pane entrambi mancanti prima del primo residente avanzato** segnala ora entrambe le esigenze senza cambiare la priorità di crescita. Aggiunti due test Application, rafforzati tre Desktop: **162 casi attesi, non eseguiti**, checkpoint confermato 140.
- 2026-10-27: **l'utente riferisce che test e validazione del precedente pacchetto sono passati** (gate `civil-growth-4`, 162 casi attesi, nove render e benchmark abitativo). Non sono stati forniti i relativi file TRX/JSON o screenshot in questa sessione; la conferma è registrata come esito riferito, senza inventare dettagli.
- 2026-10-27: implementato [nuovo confronto di scala del Pane](scenarios/BREAD_SCALE_BENCHMARK.md) per una colonia sintetica da 220 abitanti: una filiera completa, due immediate, seconda dopo 15 minuti. Misura crescita, carenze, lavoratori, materiali e controlla tutti i 1.800 passi; aggiunto `--benchmark-bread-scale` al validatore con report JSON. **Nuovo gate da eseguire**; regole, GUI e salvataggi invariati.
- 2026-10-27: l'utente ha riferito **"test passati"** anche dopo l'introduzione di `--benchmark-bread-scale`: validazione del pacchetto precedente confermata dall'utente, senza allegati TRX/JSON da cui leggere i risultati quantitativi.
- 2026-10-27: la nuova diagnosi di `GameView.BreadSupply` espone domanda, capacità nominale, produzione prevista, deficit e minimo/addizionale di addetti alla produzione del Pane (per scenario con una sola ricetta Pane). La UI temporanea visualizza l'entità del deficit quando i Forni sono insufficienti, senza confondere le carenze di input. Un test Application nuovo usa 220 abitanti, un/due Forni e ingredienti assenti; test Desktop esteso per sidebar compatta. **163 casi xUnit attesi, non ancora eseguiti**. Regole `civil-growth-4`, save e arte 8-bit invariati.
- 2026-10-27: nuovo [modello di capacità dell'intera filiera Pane](scenarios/BREAD_CHAIN_CAPACITY.md) nel read model Application, con domanda/produzione nominale per Grano, Acqua, Farina e Pane, addetti teorici necessari ed equivalente di Pane/s dell'anello limitante. Due Forni assegnati ma un solo Mulino/Pozzo e due Fattorie danno capacità dei Forni **6 Pane/s**, ma limite della catena **3 Pane/s**. Se resta soltanto una Fattoria, limite teorico **1,5 Pane/s**. Riusata e rafforzata la prova Application esistente, **sempre 163 test attesi**, nuovi assert in attesa di gate. L'alternativa snella (+4 lavoratori, 360/250 materiali) è ipotesi da misurare, non meccanica approvata. GUI attuale e futuro stile 8-bit invariati.
- 2026-10-27: **M4.7, benchmark snello implementato** nello stesso `--benchmark-bread-scale`: quarto `lean-chain` con +1 Fattoria, +1 Pozzo, +1 Mulino, +1 Forno acquistati e impiegati tramite `GameSession` (+4 addetti, 360 legno lavorato, 250 pietra). Aggiunti al JSON attività dei quattro anelli, Pane prodotto/consumato e scorte intermedie; validatore esteso a quattro strategie con controlli di costi, forza lavoro e contabilità per 1.800 passi. **Non compilato né validato qui con .NET**; nessun xUnit aggiunto, totale atteso ancora 163. [Specifica e limiti](scenarios/BREAD_SCALE_BENCHMARK.md). Nessuna variazione a regole, snapshot o GUI.
- 2026-10-27: **utente riferisce «test passati» per v17**, quindi diagnostica Forno/filiera e benchmark snello da 220 abitanti superano il gate secondo la sua esecuzione. Non sono allegati i file grezzi e non conosciamo i numeri effettivi delle quattro strategie; la conferma non è presentata come misurazione eseguita da questo ambiente.
- 2026-10-27: **M4.8** nuovo `--benchmark-bread-high-density`: usa la stessa logica del confronto già confermato, ma con **320 abitanti**, 11 Case grandi (360 posti), quattro filiere Cibo e un orizzonte di 1.800 secondi. Le quattro strategie permettono di vedere il comportamento della snella oltre il suo limite teorico medio di 4,5 Pane/s; il JSON include tempi per **340/350** residenti, costi, scorte, addetti, razioni e controlli di riconciliazione. [Scheda](scenarios/BREAD_HIGH_DENSITY_BENCHMARK.md). **Non ancora compilato/eseguito con .NET**; restano 163 test xUnit attesi, nessuna nuova regola/salvataggio/GUI.
- 2026-10-27: **v19, solo documentazione**: creato il [dossier D0](design/PRE_M5_DECISION_GATE.md), con elenco analitico dei 43 tipi strutturali raffigurati nei riferimenti, riconciliazione delle 12 corrispondenze con il catalogo corrente, scelte da approvare su vittoria, isole e popolazione militare e controlli di fattibilità. È una preparazione reversibile mentre manca la verifica Windows di v18; nessuna scelta di gameplay diventa automaticamente approvata.
- 2026-10-27: **v20, solo documentazione**: [prefattibilità M5.1](design/M5_1_INDUSTRY_PREFEASIBILITY.md) con due microfiliere proposte (ferro/carbone e oro), terza fase di contesa/scala, valori di costo e resa etichettati come ipotesi A/B, scelte di deposito e migrazione S1/S2, test e criteri di gate. Nessun file di codice, test, validazione eseguibile o salvataggio cambia rispetto a v18. **Non approvare automaticamente prezzi, ricette, dipendenze o scenario nuovo.**
- 2026-10-27: **v21, solo documentazione**: [prefattibilità M5.2](design/M5_2_FORGE_MINT_PREFEASIBILITY.md) con Forgia/Zecca, Spade/Monete, costi/ricette ipotetiche A/B e due strategie per il Carbone. Una miniera da 2/s non soddisfa quattro consumatori nominali da 1/s; alternativa a fasi contro seconda miniera. Aggiunta decisione D0.8 (Monete come scorta mirata/valuta generale; Spade consumate/dotazione). Nessuna modifica a Core, GUI, test, validator, regole `civil-growth-4` o snapshot rispetto a v18. **Non è un nuovo gate di test.**
- 2026-10-27: **utente conferma «test passati» per v21**: registrato come superato il gate del **codice v18** incluso in v21, con il terzo benchmark M4.8 e 163 casi xUnit attesi. Nessun TRX/JSON allegato; i risultati numerici delle strategie non sono noti.
- 2026-10-27: **v22, nuova GUI provvisoria da verificare**: stati del pulsante Costruisci distinti (`:pointerover`, `:pressed`, `:disabled`), riquadro sempre visibile per prerequisiti/blocchi sopra l'azione e compattazione di margini, card, lista, sidebar e footer. Aggiunti due casi geometrici per 1040×740/1240×850 e un test cromatico di hover/disabilitato: **166 casi xUnit attesi**, non eseguiti in questo ambiente. Core, regole, salvataggi e benchmark M4.8 immutati.
- 2026-10-27: **utente conferma «test passati» per v22**. Registrare la validazione come conferma dell'utente, senza TRX/PNG allegati. Dopo l'uso segnala che l'hover di Costruisci sposta gli elementi precedenti.
- 2026-10-27: **v23, regressione hover**: il bordo applicato da `Button.primary.build:pointerover` passava a 2 pixel; ora `Button.primary.build` fissa il bordo a 1 pixel per normale, hover, pressione e disabilitato. Il test Desktop confronta geometria di selettore quantità, costo e pulsante prima/dopo hover e dopo esaurimento dei materiali a **1040×740 / 1240×850**. Un nuovo caso teorico porta a **167 xUnit attesi**, ancora da eseguire; nessuna variazione a regole o salvataggi.
- 2026-10-27: **v23, studio M5.3**: [prefattibilità di Caserma e reclutamento](design/M5_3_ARMY_PREFEASIBILITY.md), invariante delle persone e alternative di addestramento, test futuri e decisioni D0.4/D0.8. Solo documentazione, non approvate Spade per soldato, tempi, perdite o razioni; nessun codice militare.
- 2026-10-27: **utente conferma «test passati» per v23**. L'hover non sposta più gli elementi; 167 casi attesi superati secondo la verifica dell'utente, senza allegati grezzi.
- 2026-10-27: **v24/M4.10 implementata**: accelerazione 1,15× per ogni Casa piccola oltre la prima, 1,30× per ogni Casa grande, fino a 2×. L'accumulo è in ventesimi, conserva i resti dopo la nascita, non avanza quando bloccato e si salva nella nuova tabella opzionale SQLite `population_growth_fraction` mantenendo `user_version=2`. Regole `civil-growth-5` con upgrade da `-4`, backup e ripresa in pausa. Forecast/UI mostrano velocità e stima. Vecchi benchmark M4 eseguibili con `--rules legacy`, nuovo confronto `--benchmark-housing-accelerated`; **174 casi xUnit attesi, non eseguiti qui**. Solo dopo il gate Windows valutare ulteriori economie M5.
- La bozza Windows Forms e i relativi test erano conservati in `legacy/winforms` nel repository originario; la cartella non è presente nell'archivio ricevuto e non è necessaria alla soluzione definitiva.
- Le nove immagini di riferimento sono in [docs/references/screenshots](references/screenshots/), indicizzate nel [documento dei riferimenti](references/README.md). Le cinque nuove immagini di risorse e strutture hanno un'[analisi dedicata](references/RESOURCE_AND_PRODUCTION_CHAINS.md).
- La prova è in [experiments/stack-probe](../experiments/stack-probe/README.md): quattro progetti, interfaccia Avalonia, modello ridotto, salvataggio SQLite e 23 test. La sua soluzione non è quella definitiva.
- La [soluzione definitiva](../SimRisorse.slnx) contiene Core, Application, Infrastructure, Desktop, Simulation e quattro progetti di test: nove progetti senza riferimenti a bozza o esperimento.
- Il simulatore esegue lo scenario economico senza finestre; non è ancora uno strumento completo di ricerca delle strategie o bilanciamento.
- Il default desktop è lo scenario `civil-growth` con regole **`civil-growth-5`**: dodici tipi di struttura, nove risorse e otto abitanti iniziali. Foundation rimane eseguibile con `--scenario economy-foundation`. Catalogo completo, vittoria, classifica, esercito e isole non sono implementati. Nessun commit o release creato per l'incremento nuovo.

## Vincoli da mantenere

1. Il giocatore vuole costruire tutti i tipi di edificio; la formulazione precisa della vittoria resta aperta.
2. La gestione avviene tramite elenco, senza collocazione degli edifici su una mappa.
3. Lavoratori, soldati, castelli e missioni sulle isole appartengono alla visione.
4. Il gioco si ferma in pausa e quando non si gioca; non esiste produzione offline.
5. La classifica misura il tempo di gioco; otto ore efficienti e oltre 32 meno efficienti sono obiettivi di bilanciamento.
6. Le immagini sono riferimenti: valori, colori e significato dei pulsanti non sono automaticamente regole approvate.
7. La futura GUI deve usare pixel art **retro 8-bit essenziale**, conservando lettura di consumi, produzione e motivi di blocco. Gestione per elenco, senza mappa di posizionamento.

## Decisioni architetturali

- Accettati: monolite modulare, funzionalità verticali e Core indipendente; tempo esplicito e riproducibilità; catalogo versionato; modelli espliciti per popolazione e progressione militare.
- **Adottato dopo la prova M1:** C# su .NET 10, Avalonia/MVVM e SQLite tramite Microsoft.Data.Sqlite, come registrato nell'[ADR 0005](adr/0005-desktop-stack-candidate.md). Windows è la prima piattaforma verificata; Linux/macOS restano da provare.
- Core e Application usano solo framework e riferimenti interni. La baseline corrente usa Avalonia 12.1.4, incluso headless, Microsoft.Data.Sqlite 10.0.14 e xUnit 3.2.2 per i soli test Desktop. La baseline storica M3 e la prova M1 conservano il proprio significato nei rispettivi report.
- L'[ADR 0006](adr/0006-economic-step-and-resource-precision.md) fissa millesimi di risorsa, passo di un secondo e prenotazioni per priorità. Nuovi edifici e risorse sono definizioni, non nuovi enum o sottoclassi.
- L'[ADR 0007](adr/0007-desktop-persistence-and-lifecycle.md) fissa persistenza, gestione del tempo reale e chiusura. Formato SQLite e limiti sono in [SAVE_FORMAT.md](SAVE_FORMAT.md).
- L'[ADR 0008](adr/0008-avalonia-12.md) sostituisce la sola baseline dei pacchetti dell'ADR 0007, su richiesta dell'utente. Migrazione e aggiornamento SQLite sono completati; nessun cambiamento a snapshot v1 o regole Economy Foundation.
- L'[ADR 0009](adr/0009-civil-growth-and-save-generations.md) introduce crescita civile, prenotazione del cibo e snapshot/file v2, mantenendo il vecchio scenario v1 in uno slot distinto. "Nuova partita" conserva lo scenario corrente.
- L'[ADR 0010](adr/0010-storage-capacity-and-rules-conversion.md) rende la capacità derivata dagli edifici (`StoragePerBuilding`) e introduce conversioni di regole esplicite in `Application/Scenarios/ScenarioUpgrades`, con copia di sicurezza nell'adattatore SQLite.
- L'[ADR 0011](adr/0011-advanced-housing-and-bread-rations.md) fissa la regola delle razioni avanzate, la Casa grande, la ricetta Pane e la conversione `civil-growth-3 → civil-growth-4` senza riclassificare scorte alimentari pregresse.
- L'[ADR 0012](adr/0012-retro-8bit-desktop-visual-language.md) registra la scelta visiva 8-bit; nessuna libreria nuova, asset o GUI definitiva introdotta finora.
- Il contenuto di tutti gli ADR è nell'[indice](adr/README.md). "Accettato" non significa "implementato".

## Contesto funzionale di M4 e checklist storica

**La checklist cronologica sottostante documenta prove già affrontate nei pacchetti precedenti.** Per l'azione immediata valgono lo stato e il gate **v27/M4.13** indicati all'inizio di questo handoff, non le vecchie indicazioni per v23.

Lo [scenario civile eseguibile](scenarios/CIVIL_GROWTH.md) conserva la base economica e aggiunge casa piccola con 8 posti, un arrivo ogni 60 secondi nutriti con spazio libero e consumo di 0,010 cibo per abitante/s. Fame, case piene e pausa fermano il progresso; nessun decesso o malus produttivo. I valori sono adottati per questo incremento e restano provvisori, senza estenderli implicitamente al mantenimento militare.

Le nuove schermate di risorse e i diagrammi precisano filiere civili, alimentari e militari. La UI M3 mostra già flussi previsti `/s`; i valori del gioco di riferimento non hanno invece un'unità temporale dimostrata. I diagrammi non forniscono costi, durate o ricette numeriche: le nuove famiglie richiedono una scheda di scenario esplicita.

La [proposta originaria](scenarios/CIVIL_GROWTH_DRAFT.md) rimane un documento storico. Le cronologie eseguibili sono nel simulatore: `bootstrap` avvia pescatore/deposito al tempo zero e casa al secondo 61, con primo arrivo al 121; `recovery` parte dalla casa, esaurisce il cibo, poi riavvia la filiera al secondo 600 e ha un nuovo arrivo al 662. I risultati sono verificati, non dimostrazioni di ottimalità.

Il magazzino è completato: capacità effettiva = 1.000 + 500 × magazzini per ogni risorsa, attiva dal secondo successivo. La cronologia `storage` del simulatore lo dimostra al secondo 1200, con legno lavorato, pietra e cibo pieni e produzione ripresa dopo la costruzione.

La filiera del pane è implementata: 2 fattorie + pozzo + mulino + forno (5 lavoratori, 420 legno lavorato + 280 pietra) producono al massimo **3 Pane/s**, in aggiunta a 1 Cibo/s della filiera del pesce con 2 lavoratori. La cronologia `bread` avvia la catena al secondo 600; senza Magazzino la capacità Pane 100 lascia il forno fermo a 99. La cronologia `advanced` aggiunge Casa grande al secondo 760: 48 posti in totale e progressione verso 48 abitanti, con razioni Pane per i residenti oltre 16. Il prerequisito Forno da solo non sostituisce l'obbligo alimentare.

Passi successivi:

- **Gate tecnico attuale:** eseguire `validate.cmd` su Windows per **v27**, con **177 casi xUnit attesi** (nessun nuovo test rispetto a v25); verificare manualmente pittogrammi e contrasto, la diagnosi fissa e la geometria immobile di Costruisci a **1040×740 / 1240×850**. Poi eseguire, se desiderato, `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\report-m4.ps1` e confrontare i dati misurati con M4.13. I gate fino a v24 sono **riferiti superati dall'utente**, senza report numerici allegati. Recuperare i JSON grezzi quando disponibili, senza inventare valori o richiedere un nuovo gate per considerarli riferiti superati.
- Verificare in particolare razioni Cibo/Pane per 16→17 abitanti, forno quando Pane è pieno, recupero dopo carenza, UI con 12 edifici e 9 risorse, conversione `-1/-2/-3 → -4` e copia originale prima del primo salvataggio.
- Ispezionare il nono render `civil-advanced-bread-compact.png` a 1040×740 con residenti avanzati: nessuna sovrapposizione tra fabbisogno, diagnosi, nota sulla pausa e blocco di navigazione inferiore. Il test verifica programmaticamente la posizione dei due testi; l'ispezione umana rimane necessaria.
- Confermare i tre nuovi test di replay: avanzamenti da un secondo e disomogenei, pausa/salvataggio al tempo 879,5 s, arrivo a 880 s e ripresa esatta a due ore, senza divergenze nelle risorse o negli edifici.
- Confermare le nuove prove di ripresa: Pane prodotto durante una carenza, arrivo al passo successivo e razione del nuovo abitante ancora un passo dopo; Cibo ripristinato dal Deposito, senza consumo anticipato di Pane. Verificare anche lo **stato Pane tipizzato** e la diagnosi simultanea quando Cibo e Pane mancano prima del primo arrivo avanzato.
- Misurare il ritmo su più cronologie e rivedere la capacità Pane 100 se il forno sempre fermo rende il comportamento poco leggibile; non cambiare i valori senza nuova versione di regole e prova di regressione.
- Prima di aggiungere Carne o più ricette al Deposito alimentare, risolvere la [decisione di ruolo](scenarios/MEAT_DESIGN_REVIEW.md) e definire un vantaggio misurabile. Non aumentare la complessità del motore mentre il gate delle regole correnti è aperto.
- Eseguire `--benchmark-housing`, verificare i tre profili rispetto ai tempi attesi e registrare il JSON in `.local-cache/m4/housing-benchmark.json`. Confrontare il risparmio dei materiali con il ritardo della costruzione della prima Casa grande. L'esperimento non ricerca una strategia ottima.
- Controllare `ReconciledSeconds = 28.800` in ciascuna strategia e il test Core a 220 abitanti con uno o due Forni: misurano portata produttiva e coerenza delle previsioni.
- Acquisire alla prossima validazione il JSON `--benchmark-bread-scale` **a quattro strategie**, già dichiarato superato dall'utente in v17 ma non allegato; affiancarlo al nuovo JSON `--benchmark-bread-high-density` (320 abitanti). Non confondere con la versione storica a tre strategie.
- Dopo il gate, **analizzare i due JSON di scala** (220 e 320 abitanti): `lean-chain` risparmia un lavoratore, 60 legno lavorato e 30 pietra rispetto al raddoppio integrale, ma il limite teorico medio resta 4,5 Pane/s. Non dichiarare la popolazione finale o il pareggio economico effettivo senza leggere gli output; lo scenario ad alta densità distingue eventuali carenze oltre la soglia dei 308 abitanti.
- Diagnostica dei colli di bottiglia: già implementati i nomi delle risorse insufficienti o prive di spazio nel dettaglio di ogni struttura e la produzione parziale nell'elenco. Dopo il gate una successiva estensione potrà proporre quantità o suggerimenti di costruzione.
- Decidere demolizione e conseguenze su personale, capacità e scorte (Q-08) prima di abilitarla nella UI.
- Usare lo [studio di demolizione](scenarios/DEMOLITION_M4_REVIEW.md) per decidere il rimborso e la prevenzione delle filiere irrimediabilmente bloccate, senza cambiare il Core prima del gate.
- Dopo il gate, prototipare **una schermata 8-bit** con Casa grande, Forno, Cibo, Pane e diagnostica, verificando le stesse risoluzioni e mantenendo l'attuale flusso da elenchi.
- Prima del codice M5.1, discutere il [dossier industriale](design/M5_1_INDUSTRY_PREFEASIBILITY.md): separazione M5.1a/b/c; validazione delle proposte A/B sui materiali; approvazione della strategia di migrazione dei checkpoint civili S1/S2; vincoli di capacità e concorrenza sul carbone. È *progettazione*, non autorizzazione a cambiare le regole durante M4.
- Prima del codice M5.2, discutere il [dossier su Forgia e Zecca](design/M5_2_FORGE_MINT_PREFEASIBILITY.md): quantità ipotetiche di Spade/Monete, costo della seconda miniera di Carbone rispetto alle lavorazioni a fasi e decisione D0.8 sul significato delle due risorse. Non anticipare soldati, navi o monete generalizzate con semplici definizioni di catalogo.
- Ampliare le cronologie riproducibili e i controlli di avviabilità/recupero, mantenendo i test di tempo, contesa, pause e salvataggi.
- Ripristinare l'audit CLI quando l'accesso NuGet .NET funziona; conservare il controllo puntuale e le evidenze di aggiornamento già prodotti, senza aggirare TLS.

Esercito, navigazione e isole rimangono M5; i nove elementi decorativi sono riferimenti per la progressione avanzata, non contenuti già approvati o richiesti per la prima estensione civile. La [matrice D0](design/PRE_M5_DECISION_GATE.md) espone tutte le opzioni da discutere, senza anticipare il catalogo finale.

## Avvio e verifiche correnti

```powershell
.\validate.cmd -OfflinePackageSource .local-cache/nuget
.\run-game.cmd
```

**Checkpoint M4 confermato dall'utente:** test e validazione dichiarati superati fino al **pacchetto v22**, comprensivo di M4.8/v21 e GUI compatta (**166 casi xUnit previsti**). I report grezzi non sono allegati. L'ultimo report numerico ispezionato direttamente in una sessione precedente era di **140 test superati** (59 Core, 33 Application, 22 Infrastructure, 26 Desktop). **Ultimo incremento eseguibile: v23**, che corregge solo l'hover e rafforza il test GUI (**167 casi xUnit attesi, non eseguiti qui**). Studi D0/M5.1/M5.2/M5.3 restano documentali. Dettagli nel [report M4](validation/M4_CIVIL_GROWTH.md).

La sessione parte/riprende in pausa. Il file personale civile è `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`; i checkpoint `civil-growth-1`, `-2` e `-3` sono convertiti in memoria verso `-4` e, al primo salvataggio, l'originale resta in `civil-growth.<regole>.backup.sqlite3`. Il Cibo già presente non è trasformato in Pane. Foundation mantiene `game.sqlite3`, accessibile con `run-game.cmd --scenario economy-foundation`. Checkpoint ogni minuto, manuali e alla chiusura; nessuna produzione offline. La chiusura fallita conserva la finestra e offre uscita esplicita senza salvare. File incompatibili all'avvio non vengono sostituiti da autosalvataggi della sessione iniziale di ripiego.

Artefatti dell'ultimo gate ispezionato in `.local-cache/m4/`: `validation.json`, otto PNG storici, pubblicazione, nove JSON delle simulazioni e risultati/log/database sotto `runs/<id>/`. Il gate successivo include dodici simulazioni, **tre benchmark** (abitazioni, Pane a 220, Pane a 320 abitanti) e nove PNG isolati in `runs/<id>/screenshots/`. I percorsi sono elencati in `validation.json`; validazione e `run-game.cmd --smoke-test` non usano file personali. I risultati del gate confermato dall'utente non sono presenti nell'archivio sorgente.

L'audit CLI è stato ritentato ma non completato per errore TLS. Un confronto puntuale con i dati ufficiali VulnerabilityInfo di NuGet ha invece esaminato le 63 coppie pacchetto/versione dei lock della soluzione definitiva: dopo l'aggiornamento SQLite nessuna corrisponde agli intervalli segnalati. Dati, report prima/dopo e limite del controllo sono in `.local-cache/avalonia12-audit/` e nel report della migrazione. Pacchetti scaricati via HTTPS standard in `.local-cache/avalonia12-feed/`, restore da sorgenti locali, nessuna modifica globale.

Linux/macOS, self-contained, spegnimento improvviso e migrazioni di formato non sono verificati. La CI resta da introdurre.

## API e limiti correnti

- `GameRuntime` possiede una sola sessione e serializza modifiche e letture. Gli handler delle funzionalità sono in `Application/Features/`.
- `GameSession` è un aggregato a singolo proprietario; può essere usato direttamente nei test, ma i chiamanti concorrenti passano dal runtime.
- Acquisti e assegnazioni hanno quantità intere e sono atomici. Gli errori attesi sono codici `GameError`.
- `ResourceAmount` rappresenta 0,001 unità su `long`; conversioni con più decimali vengono rifiutate, non arrotondate.
- Il pianificatore usa input e spazio iniziali, priorità crescente e identificatore ordinale; le previsioni usano la stessa contesa. Le attività successive possono restare senza input.
- Passo economico di un secondo, tempo residuo conservato in tick. Massimo 24 ore per singola chiamata di avanzamento; durata totale non limitata a 24 ore.
- Il Core valida snapshot e riprende in pausa. `GameSession.Population` è la popolazione effettiva; `GameScenario.Population` è la dotazione iniziale. Capacità abitativa e capacità delle risorse dipendono dagli edifici: `GameScenario.Capacity` è solo la base, `GameSession.StorageCapacity` quella effettiva.
- `GameRuntime.TryRestore` applica le conversioni dichiarate solo per lo stesso scenario e verso le regole attive; `PersistenceResult.UpgradedFromRules` segnala la conversione.
- `ForecastEconomy()` comprende attività delle strutture e piano civile. La proiezione include consumo effettivamente prenotabile e fabbisogno alimentare distinto, così una razione mancante non appare consumata.
- Foundation emette snapshot v1 senza crescita; Civil emette v2 con `GrowthProgressSeconds` obbligatorio, validato rispetto all'intervallo. Il caricamento di regole diverse non cambia automaticamente lo scenario.
- Catalogo in memoria: riferimenti e cicli edilizi controllati, nessun fingerprint automatico. `CatalogReachability.Analyze` verifica staticamente che ogni struttura sia costruibile e ogni ricetta avviabile; non considera tempo, persone o contesa e non è imposto nel costruttore dello scenario.

## Verifiche precedenti M2 e comandi

Build Release: **zero errori e avvisi**. **45 test superati**: 36 Core e 9 Application/architettura. Verificate simulazioni di 60 secondi, 8 e 32 ore e isolamento delle configurazioni legacy/M1. Dettagli nel [report M2](validation/M2_CORE.md).

Controlli finali: 22 documenti con 102 collegamenti locali validi, riferimenti dei cinque progetti e lock file coerenti. Contenuto di 26 file storici confrontato con Git normalizzando i fine riga; SHA-256 dei quattro screenshot invariato. Verificato anche il comando `dotnet run` documentato per lo scenario di 60 secondi.

L'audit online è stato tentato ma non completato per errore TLS dell'ambiente. Nessuna verifica dei certificati è stata disattivata. La validazione funzionale offline è completa; i pacchetti esterni dei test e il runtime non sono dichiarati verificati dall'audit.

```powershell
.\validate.cmd -OfflinePackageSource "$env:USERPROFILE\.nuget\packages"
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --hours 8
```

Artefatti in `.local-cache/m2/`: TRX e JSON delle simulazioni. Core e Application non hanno pacchetti esterni; i test usano versioni centralizzate e lock file. La CI resta da introdurre.

## Decisioni di prodotto ancora aperte

Consultare Q-01–Q-10 nel [game design](GAME_DESIGN.md). In particolare non assumere già approvati: possesso simultaneo di tutti gli edifici, isole obbligatorie per vincere, popolazione civile/militare condivisa, consumo alimentare, demolizione con rimborso, missioni deterministiche o classifica pubblica.

## Pulizia effettuata

| Prima | Adesso |
| --- | --- |
| `Gestionale.sln` | `legacy/winforms/Gestionale.sln` |
| `Gestionale/` | `legacy/winforms/Gestionale/` |
| `TestProject1/` | `legacy/winforms/TestProject1/` |
| `screnshot/` | `docs/references/screenshots/` |
| `.vs/`, `bin/`, `obj/` e preferenze locali | `.local-cache/`, esclusa da Git |
| Cartelle vuote rimaste dall'analisi Graphify | `.local-cache/graphify-out/` |

La pulizia delle cache è reversibile. La cancellazione ricorsiva è stata rifiutata dal controllo automatico dei permessi; i file sono stati spostati all'interno del repository, non eliminati. I vecchi output di build non devono essere usati per verificare il nuovo percorso della soluzione: si rigenerano con un build esplicito quando necessario.

## Verifiche precedenti M1 e comandi

La validazione finale ha prodotto zero errori e avvisi di build, **23 test superati su 23**, due immagini headless e una pubblicazione framework-dependent avviata su Windows con codice 0. Il [report M1](validation/M1_STACK_PROBE.md) registra dettagli e limiti.

Controlli finali del repository: 19 documenti con 81 collegamenti locali validi, riferimenti dei quattro progetti della prova validi, cinque ADR accettati e integrità SHA-256 dei 30 file preservati confermata. Anche il comando di avvio interattivo è stato eseguito nella modalità smoke con esito positivo.

I test coprono ricette, carenze, capacità, conservazione del personale, tempo frazionario, ripetibilità su otto ore, cronometro a 32 ore, round trip SQLite, rollback di una scrittura fallita, dati incoerenti e UI reattiva durante I/O. Il test di otto ore non verifica la durata del gioco completo.

Riproduzione offline dalla radice, con pacchetti già presenti nella cache indicata:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-stack-probe.ps1 -OfflinePackageSource "$env:USERPROFILE\.nuget\packages"
```

Dopo la validazione, avvio interattivo con `scripts/run-stack-probe.cmd`. Artefatti in `.local-cache/stack-probe/`; salvataggio personale in `%LOCALAPPDATA%/SimRisorse/StackProbe/save.sqlite3`. Database dei test separati e temporanei.

Non verificati: Linux/macOS, audit online, pacchetto self-contained, gestione completa della sospensione del sistema, compatibilità futura dei salvataggi e bilanciamento del catalogo completo. Lo stack è tecnicamente adottato, non certificato su tutte le piattaforme.

## Verifiche precedenti M0

Verifiche eseguite con esito positivo il 2026-10-26:

- Confronto SHA-256 di 30 file preservati: soluzione, sorgenti, test, risorse dei form, attributi Git, licenza e quattro immagini. Il solo `.gitignore` è stato modificato intenzionalmente.
- Risoluzione dei due progetti nella soluzione legacy e del riferimento dal progetto di test all'applicazione.
- Controllo di 16 documenti Markdown: 58 collegamenti locali validi; nessuno spazio finale o terminatore mancante.
- Al completamento M0 gli ADR erano quattro accettati e stack proposto; al completamento M1 l'ADR 0005 è passato ad accettato con evidenze.
- `git diff --check` senza errori e `git check-ignore` con esito atteso per cache, dati locali e cache Graphify.

I file trasferiti appaiono come eliminazioni dai vecchi percorsi e file nuovi nei nuovi percorsi nello stato Git non in staging; gli hash confermano che il loro contenuto è invariato. Non è stato eseguito staging.

Durante M0 non furono eseguiti build e test .NET. Le verifiche M1 riguardano l'esperimento, non i due test storici della bozza: questi ultimi verificano assegnazioni e nomi, non la simulazione.

## Problemi della bozza da non trasferire

- Il tempo viene letto direttamente in `Costruzione.CicliTotali()`.
- I pulsanti aggiungono edifici senza addebitare costi.
- `Magazzino.CalcoloDepositoCiclo()` ricalcola totali dalla creazione e può accreditare produzione senza input sufficienti.
- La capacità viene calcolata ma non applicata come limite coerente ai flussi.
- I campi pubblici permettono modifiche arbitrarie allo stato.

Questi punti sono osservazioni sul codice, non correzioni già eseguite.

## Aggiornamento alla prossima sessione

Registrare secondo lo schema fisso **dove siamo / cosa abbiamo fatto / cosa implementiamo / cosa ci aspetta / cosa resta aperto**. Separare test confermati dall'utente, test eseguiti direttamente e test soltanto predisposti; indicare versione del checkpoint, effetti su regole e salvataggi. Conservare nel changelog le modifiche concluse e nella roadmap quelle future.

## Checkpoint v36 del 2026-10-28

Aggiornato il **2026-10-28**. Punto di ripresa corrente; per le fasi precedenti vedere lo [storico dell'handoff](HANDOFF_HISTORY.md), il [changelog](../CHANGELOG.md) e i [rapporti di verifica](README.md#verifiche).

## Stato della baseline e validazione pendente

**M0–M4 completate; M5.1a Ferro e M5.1b Oro verificate.** Scenario `civil-growth-7`, **17 tipi di edifici e 14 risorse**. I cinque tipi futuri della prima campagna sono Forgia, Zecca, Caserma, Cantiere e Castello.

L'ultimo `validate.cmd` **riportato dai documenti della copia locale dell'utente** ha completato restore, build Release con **zero avvisi/errori**, **206/206 test**, 16 cronologie più crescita accelerata, quattro benchmark e smoke nativi Windows dei due scenari. I relativi TRX e artefatti non erano nell'archivio, quindi in questo ambiente non è stata possibile una verifica indipendente di quell'esecuzione.

**Nuovo incremento M5.1c preparato:** quinto benchmark `--benchmark-industry-scale` con cinque profili di contesa del Carbone, controllo dei flussi a ogni secondo e replay a tempi frazionari. Sono stati cambiati solo simulatore, validatore e documentazione; **nessuna regola, GUI, migrazione o test xUnit è stata modificata**. [Report di progetto e valori teorici attesi](validation/M5_1C_INDUSTRY_SCALE.md). **La build e il nuovo gate Windows non sono ancora stati eseguiti.**

**Preparazione documentale successiva (v31):** ampliato il [dossier M5.2](design/M5_2_FORGE_MINT_PREFEASIBILITY.md) con scelta A/B da sottoporre all'utente, ipotesi di Spade/Monete, priorità, capienza a lotti, test F1–F10 e confronto **241 s con una miniera a fasi / 122 s con due miniere** per 240 Spade + 360 Monete, a condizioni sintetiche ideali. Sono **solo calcoli teorici**, non benchmark .NET; nessuna nuova regola approvata e nessuna modifica al codice rispetto alla v30.

**Prova sperimentale v32 preparata:** [ForgeMintProbe](../experiments/forge-mint-probe/README.md) è un progetto .NET separato dalla soluzione, con catalogo ipotetico in memoria, sei profili A/B, acquisti reali, contabilità a ogni secondo e replay a passi interi/frazionari. **Non modifica `src/`, `tests/`, `tools/`, `scripts/`, `SimRisorse.slnx` o il gioco**, e non è stato eseguito in .NET qui. Le stime aritmetiche 241/122 s sono state confermate da un modello distinto, non ancora dal nuovo eseguibile.

**Incremento v33 preparato:** la tabella delle risorse nel Desktop mostra ora i nomi in **ordine alfabetico italiano**. Si ordina una sola volta all'avvio; aggiornamenti e snapshot seguono ancora gli ID esistenti. Le tre card sintetiche preservano l'ordine storico. Aggiunti due casi xUnit Desktop (scenario base e civile); **non ancora compilati/eseguiti su .NET**. Il numero 206 indica esclusivamente l'ultimo gate confermato, non questa modifica.

**Conferma utente del 28/10:** «test passati» per la v33. Il log di `validate.cmd` non è stato fornito; il numero **208** rimane il conteggio **atteso** della suite dopo i due nuovi casi Desktop, mentre il rapporto storico da **206/206** è l'ultima evidenza dettagliata conservata. Non attribuire un esito alla prova ForgeMintProbe: è fuori dal gate ordinario.

**Incremento v34 preparato:** l'[esperimento ForgeMintProbe](../experiments/forge-mint-probe/README.md) include ora, oltre ai sei profili A/B, **sei guardie F8/F9**: lotti bloccati con 1–2 posti residui, lotti che riempiono esattamente il magazzino e precedenza del Carbone alle due fonderie. Ogni guardia confronta replay interi/frazionari e snapshot. **Non compilato né eseguito qui**, prezzi e significati economici restano candidati; gioco e suite xUnit invariati rispetto alla v33.

**Incremento v35 preparato (richiesta UI):** tabella Risorse con colonna «Si riempie tra» in `hh:mm:ss` a saldo netto positivo (stima a flusso costante), `00:00:00` se già piena, `—` con saldo nullo/negativo; quantità della sola tabella arrotondata all'unità, dati precisi e card invariati. Allineamento a destra di intestazioni e valori numerici. Consumi per addetto/secondo nel dettaglio Edifici resi come elenco con icona tramite `ResourceDetailTemplate`, identico ai costi e alle produzioni. Nuovo test numerico e regressioni visuali ampliate: **209 casi xUnit attesi**. **Non compilato né eseguito qui**; solo Desktop/test/documenti, nessuna nuova regola o migrazione.

**Incremento v36 preparato (correzione UI):** Risorse è ora un `ListBox` scrollabile autonomamente, con selezione persistente della riga nel medesimo ocra dell'elenco Edifici. Entrambi gli elenchi riservano **20 px** alla scrollbar verticale tramite classe `gutter-list` e intestazioni arretrate; il fondo di selezione non deve coprire la barra. Due nuovi casi Desktop a **1040×740 e 1240×850** confrontano la posizione del margine delle righe con la barra, il clic, il colore e l'avanzamento: **211 casi xUnit attesi**. **Non compilati né eseguiti qui**, in attesa di gate Windows. Nessuna modifica al motore, al catalogo o ai salvataggi.

**Decisione M5.2 ricevuta il 28/10:** l'utente ha confermato **l'intero contratto A** del [dossier Forgia/Zecca](design/M5_2_FORGE_MINT_PREFEASIBILITY.md): Forgia costo **240 Tavole/180 Pietra**, 1 addetto, **1 Ferro + 1 Carbone → 2 Spade/s**; Zecca **210 Tavole/160 Pietra**, 1 addetto, **1 Oro + 1 Carbone → 3 Monete/s**. Le Spade saranno consumate dal futuro arruolamento (quantità non ancora definita), le Monete saranno scorta per **pagamenti espliciti**, non valuta generale (costo nave rinviato). Priorità fisse Ferro 90, Oro 91, **Forgia 100, Zecca 101**, capacità standard di deposito, **lotti indivisibili**. È un'**approvazione progettuale**, non implementazione; M5.2a/b saranno i prossimi incrementi dopo i gate. Non richiedere nuovamente queste scelte.

| Suite | Test superati |
| --- | ---: |
| Core | 67 |
| Application | 61 |
| Infrastructure | 30 |
| Desktop | 48 |
| **Totale** | **206** |

Run precedente **riportata**: `899f6ff3e8ff41288330c3d26b4c4ea7`, sotto `.local-cache/m4/runs/`. Indicati **19 PNG**: 17 render richiesti dal manifest più due render del dettaglio. [Report dell'interfaccia](validation/DESKTOP_UI.md). Il nuovo benchmark non costituisce una nuova esecuzione del gate.

## Decisioni già ricevute

- **Prima campagna:** i [22 tipi della scheda dedicata](design/FIRST_CAMPAIGN_PROPOSAL.md); gli altri 21 riferimenti sono fuori.
- **Vittoria:** possedere tutti i 22 tipi, con il Castello come costruzione conclusiva; gli edifici iniziali contano.
- **Conquista:** una missione obbligatoria sblocca il Castello. Isola, costi, forze, durata ed esiti restano da scegliere.
- **Demolizione:** rinviata. Nessun rimborso approvato; Carne fuori dalla prima campagna.
- **Salvataggi S1:** continuare le partite civili con conversione e backup, senza uno slot campagna distinto.
- **Ferro e Oro:** configurazioni A approvate con risposte separate. Il Ferro mantiene la priorità sul Carbone scarso.
- **Forgia e Zecca:** l'intero contratto A approvato il 28/10, non ancora implementato; quantità future per reclute/navi non scelte.
- **Grafica:** base retro monospace approvata; menu laterale più largo, card abitanti e scorte, costi prima della produzione, icone delle risorse e hover ocra.

Non richiedere nuovamente queste scelte. Le [decisioni residue D0](design/D0_DECISION_BOARD.md) riguardano le nuove fasi, senza riaprire quelle acquisite.

## Regole economiche correnti

| Struttura | Costo: legno lavorato / pietra | Ricetta per addetto e secondo | Prerequisiti |
| --- | ---: | --- | --- |
| Miniera di ferro | 120 / 80 | → 2 minerale di ferro | Casa grande |
| Miniera di carbone | 130 / 90 | → 2 carbone | Casa grande |
| Fonderia di ferro | 200 / 150 | 2 minerale di ferro + 1 carbone → 1 ferro | Miniere di ferro e carbone |
| Miniera d'oro | 180 / 120 | → 1 minerale d'oro | Casa grande |
| Fonderia d'oro | 220 / 170 | 1 minerale d'oro + 1 carbone → 1 oro | Miniere d'oro e carbone |

Un lavoratore per edificio; scorte industriali iniziali zero, capacità 1.000 e +500 per Magazzino. Il materiale estratto è utilizzabile dal secondo successivo. Il pianificatore economico del Core resta quello esistente.

La strategia `gold` raggiunge il primo Oro a **1.502 s**, con Ferro 401, Carbone 402 e popolazione 33; a otto ore Oro e Ferro sono a 1.000, popolazione 48. Sono cronologie di riferimento, non prove di ottimalità. [Ferro](validation/M5_1A_IRON.md), [Oro](validation/M5_1B_GOLD.md).

## Compatibilità da preservare

- Slot civile e formato SQLite/snapshot **v2**. Conversioni concatenate `civil-growth-1…-6 → -7`, nuovi contenuti a zero e backup dell'originale alla prima scrittura.
- Conservare scorte precedenti, lavoratori, Magazzini, tempo e frazioni della crescita. Rifiutare scorte industriali positive prima della versione che le introduce.
- Cataloghi storici v4/v5 per i benchmark e v6 per le verifiche del Ferro; non sostituirli con il catalogo corrente.
- L'[esperimento A/B](../experiments/industry-probe/README.md) resta separato, su colonia sintetica v5.
- Nessuna produzione offline; salvataggi di test e smoke separati dalle partite personali. [Formato dei salvataggi](SAVE_FORMAT.md).

## Interfaccia corrente

Sidebar da **240 px** con Edifici/Risorse e Crescita civile; card condivise di abitanti/posti/liberi e scorte. Dettaglio: descrizione, costi in elenco, produzione o effetto, consumi. Costi e produzione condividono testo monospace normale da **12 px** e icone risorsa da **14 px**.

La tabella Risorse applica ordinamento alfabetico ai **nomi italiani mostrati** (senza distinzione maiuscolo/minuscolo, parità risolta dall'ID). Le card delle tre risorse in evidenza non vengono riordinate. L'ordinamento del catalogo, il motore e i salvataggi restano invariati.

La v35 aggiunge tempo stimato di riempimento al **saldo netto del prossimo secondo** (`hh:mm:ss`), `—` se non cresce; la stima non considera variazioni future degli addetti e dei materiali. Le cinque colonne numeriche precedenti e quella nuova sono allineate a destra; la quantità nella sola tabella è intera, nelle card resta precisa. Costi, produzioni e consumi della struttura usano lo stesso template icona + testo normale da 12 px. La **v36** separa le righe dalle scrollbar e rende selezionabili in ocra anche le risorse; l'ordinamento alfabetico resta stabile.

Hover dei pulsanti abilitati nello stesso ocra della struttura selezionata (`#E6D5AC`), testo scuro; disabilitati grigi e geometria stabile. Font, pittogrammi e regole visive sono descritti nel [contratto grafico](PIXEL_ART_UI.md).

Il [README](../README.md) include ora uno screenshot versionato in [docs/screenshots](screenshots/README.md), distinto dalle immagini del gioco di riferimento.

## Prossimo lavoro

1. **Validare la v36 in Windows** (211 casi xUnit attesi) e controllare visivamente scrollbar, colore della selezione, colonne Risorse e consumi Edifici a 1040×740 / 1240×850. Conservare l'esito Windows dichiarato per la v33; se disponibile, acquisire il riepilogo del comando completo per distinguere il gate M5.1c dai soli test xUnit. L'esperimento M5.2 è facoltativo e separato:

   ```powershell
   dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-industry-scale
   .\validate.cmd
   ```

2. Dopo il gate, **implementare M5.2a Forgia/Spade**, poi **M5.2b Zecca/Monete** secondo il [contratto già approvato](design/M5_2_FORGE_MINT_PREFEASIBILITY.md), preservando versione di regole S1, migrazione, backup, scorte e precisione. La [prova isolata](../experiments/forge-mint-probe/README.md) può informare i controlli, ma non sostituisce i gate. Per M5.3/M5.4 rimangono da scegliere i quantitativi di impiego delle Spade e delle Monete.
3. Poi popolazione militare, Caserma, Cantiere, flotta, missione obbligatoria e vittoria, secondo la [roadmap](ROADMAP.md).
4. DPI fisici, altre piattaforme e accessibilità restano verifiche di rifinitura; non considerarli già coperti dai render headless.

Per ripartire: leggere questa pagina, preservare le modifiche Git presenti e usare i comandi della [guida di sviluppo](DEVELOPMENT.md).

Riordino documentale della baseline precedente: **62 file Markdown e 468 collegamenti locali**, ancore comprese, senza errori secondo il rapporto locale dichiarato `.local-cache/docs-audit.json`. Il nuovo file M5.1c richiede di aggiornare il controllo dei collegamenti; screenshot del README preservato, 1240×850.

## Checkpoint v38 del 2026-11-02

Aggiornato il **2026-11-02**, checkpoint **v38 / effetti UI Verificati**, dopo **M5.2a Verificato**. Lo stato precedente è nello [storico](HANDOFF_HISTORY.md); il [report M5.2a](validation/M5_2A_FORGE.md) conserva misure e limiti del gate completo.

**v38, richiesta UI:** gli effetti nel dettaglio Edifici sono elenchi con lo stesso `ResourceDetailTemplate` di costi/produzione/consumi: monospace normale 12 px, icone 14 px. Magazzino con icona Magazzino e +500 capacità per ogni risorsa per magazzino; case con icona abitanti e +8/+32 posti abitativi per casa. Gli effetti restano per singolo edificio anche negli ordini multipli. **57/57 test Desktop eseguiti**, sei render ispezionati alle due dimensioni. Run `d1d84fdc8e0a4fd2b0900228ef33f987`, TRX e PNG in `.local-cache/ui-effects/runs/`, log in `.local-cache/ui-effects/test.log`. Nessun cambio di regole o salvataggi; il gate completo M5.2a qui sotto resta l'ultima esecuzione completa, precedente alla v38. Prossima funzionalità M5.2b Zecca/Monete; DPI fisici e altre piattaforme restano aperti.

## Stato verificato

M0–M4 completate; M5.1a/b e **M5.1c verificate**. **Forgia e Spade M5.2a implementate e verificate**, regole `civil-growth-8`, **18 tipi di edifici / 15 risorse**. Restano Zecca, Caserma, Cantiere e Castello per i 22 tipi della prima campagna.

Gate completo `validate.cmd` eseguito e ispezionato su Windows il 02/11: **235/235 test**, build Release con **zero avvisi/errori**, raggiungibilità dei due cataloghi, **18 cronologie più crescita accelerata**, cinque benchmark e smoke nativi dei due scenari. Run `909f039588b94b69b80b8d9e9d9bcd26`, artefatti `.local-cache/m4/`, TRX e **23 PNG** nella cartella della run (21 richiesti dal manifest più due dettagli). Quattro render Forgia/Risorse ispezionati a 1040×740 e 1240×850.

| Suite | Superati |
| --- | ---: |
| Core | 67 |
| Application | 79 |
| Infrastructure | 32 |
| Desktop | 57 |
| **Totale** | **235** |

Prima dell'incremento, anche la baseline v36 è stata verificata: **211/211**, run `11a1fbf5ceff4a6a87fd52bd1126979a`. Corrette due cause di fallimento: pennelli statici mutabili in `ResourceRow` creati fuori dal dispatcher (ora immutabili) e confronto dei font con una riga costi già scollegata dalla finestra (ora si usa la riga corrente). Il gate M5.1c ha confermato tutti i cinque profili e i replay.

## Regole e salvataggi

- **Forgia:** costo 240 Legno lavorato / 180 Pietra, 1 addetto, prerequisito Fonderia di ferro posseduta. Ricetta **1 Ferro + 1 Carbone → 2 Spade/s**, priorità **100**, dopo Ferro 90 e Oro 91.
- Spade iniziali zero, capacità **1.000 +500 per Magazzino**. Lotti indivisibili: con 999/1.000 la Forgia si ferma senza spendere input. Nessun arruolamento introdotto.
- Strategia `forge`: filiera civile e Ferro come `industry`, Forgia acquistata a 1.400 s, primo lotto a **1.401 s** (2 Spade, 31 abitanti, Ferro 299, Carbone 301); a otto ore Spade 1.000 e popolazione 48. Percorso di riferimento, non ottimizzazione o misura della durata della campagna.
- Conversione concatenata **`civil-growth-1…-7 → -8`**, Forgia a quantità/addetti zero e Spade a zero. Preservati scorte, Magazzini, persone, lavoratori, tempo e frazioni. Vecchi checkpoint con Spade positive o righe Forgia incompatibili rifiutati.
- SQLite/snapshot **v2**, stesso slot civile, caricamento in memoria e backup originale prima della prima scrittura. Nessuna produzione offline e nessun salvataggio personale usato nei test.
- Cataloghi storici v4/v5/v6 preservati; nuovo `CreateGoldForBenchmarks()` fissa v7 per M5.1c e per l'esperimento Forgia/Zecca. Non aggiungere la Zecca a `civil-growth-8`: il prossimo incremento richiede **v9**.

## Decisioni già ricevute

Non richiedere nuovamente queste scelte:

- Prima campagna con 22 tipi; vittoria col possesso di tutti, edifici iniziali validi e Castello conclusivo dopo una conquista obbligatoria. Parametri della missione ancora aperti.
- S1, parametri A Ferro/Oro e **intero contratto A Forgia/Zecca approvato il 28/10**. Zecca: 210/160, 1 addetto, prerequisito Fonderia d'oro, **1 Oro + 1 Carbone → 3 Monete/s**, priorità 101, deposito ordinario e lotti interi.
- Spade per futuro arruolamento e Monete per pagamenti espliciti, senza valuta generale. Quantità per recluta/nave da scegliere nelle fasi M5.3/M5.4.
- Grafica retro monospace, icone, card condivise e hover/selezione ocra approvati. Demolizione **Rinviata**, Carne fuori dalla prima campagna.

## Prossimo lavoro

1. **M5.2b Zecca/Monete:** implementare il contratto già approvato, regole `civil-growth-9`, conversione `-8→-9`, test di primo lotto, capienza residua 1–2, precedenza sul carbone, round trip SQLite e interfaccia.
2. **M5.2c:** confrontare nel gioco corrente produzione a fasi e parallela, con cronologia reale da nuova partita e artefatti del validatore. L'[esperimento isolato](../experiments/forge-mint-probe/README.md), eseguito separatamente il 02/11, supera sei profili A/B e sei guardie F8/F9: conferma **241/122 s** nel checkpoint sintetico v7, non nel gioco completo. Non equivale a chiudere M5.2c.
3. Poi M5.3 popolazione militare/Caserma, M5.4 Cantiere/flotta, missione, conquista e Castello secondo la [roadmap](ROADMAP.md). I contratti a valle mancanti richiedono scelte prima del codice.

Restano aperti DPI fisici, altre piattaforme e accessibilità; i render headless e gli smoke Windows non li coprono. Per i comandi leggere la [guida di sviluppo](DEVELOPMENT.md). Gli esiti precedenti e la conferma utente v33 restano nello storico, senza attribuire loro l'esecuzione attuale.

## Checkpoint v39 del 2026-11-02

Aggiornato il **2026-11-02**, checkpoint **v39 / M5.2b Zecca e Monete Verificato**. [Report corrente](validation/M5_2B_MINT.md), [storico](HANDOFF_HISTORY.md), [roadmap](ROADMAP.md).

## Stato verificato

M0–M4 completate; M5.1a/b/c e **M5.2a/b Verificate**. Regole **`civil-growth-9`**, **19 tipi di edifici / 16 risorse**. Restano Caserma, Cantiere e Castello per i 22 tipi della prima campagna. **M5.2c resta da realizzare**: l'integrazione di Zecca non chiude da sola l'intera M5.2.

Gate completo `validate.cmd` eseguito il 02/11: **258/258 test**, build Release con **zero avvisi/errori**, raggiungibilità dei due cataloghi, **20 cronologie più crescita accelerata**, cinque benchmark, pubblicazione e smoke nativi Windows dei due scenari. Run **`b478adfdae134725b7610d6acefee7e8`**. Artefatti `.local-cache/m4/validation.json`, `mint-validation.log`, `mint-start.json`, `mint-8-hours.json`; TRX e **33 PNG** sotto `runs/<id>/` (31 richiesti, più due dettagli). Quattro nuovi render Zecca/Risorse ispezionati a 1040×740 e 1240×850. I sei render degli effetti v38 sono ora richiesti anche dal manifest del gate completo.

| Suite | Superati |
| --- | ---: |
| Core | 67 |
| Application | 96 |
| Infrastructure | 34 |
| Desktop | 61 |
| **Totale** | **258** |

## Regole e salvataggi

- **Zecca:** costo **210 Legno lavorato / 160 Pietra**, 1 addetto, prerequisito Fonderia d'oro posseduta. **1 Oro +1 Carbone → 3 Monete/s**, priorità **101**, dopo Ferro 90, Oro 91 e Forgia 100. Monete iniziali zero, capacità **1.000 +500 per Magazzino**, lotti interi. Con 1–2 posti residui si ferma senza spendere input; nessuna valuta generale o spesa navale introdotta.
- **Forgia:** 240/180, 1 addetto, Fonderia di ferro, **1 Ferro +1 Carbone → 2 Spade/s**, priorità 100 e deposito ordinario. Spade per il futuro arruolamento, ancora assente.
- Strategia `mint`: filiera civile/ferro, Oro a 1.500 s, Zecca a **1.800 s**; primo lotto a **1.801 s**, Monete 3, popolazione 41, Oro 299, Ferro 700, Carbone 401. A otto ore Monete **999/1.000**, metalli/Carbone 1.000, popolazione 48. È un percorso di riferimento senza Forgia attiva, non un confronto delle due politiche complete.
- Conversione **`civil-growth-8 → -9`**, concatenata da v1: Zecca a quantità/addetti zero, Monete zero. Conserva Spade frazionarie, Forgia/addetto, scorte, Magazzini, persone, tempo e frazioni della crescita. Rifiuta Monete positive o righe Zecca incompatibili prima dell'introduzione.
- SQLite/snapshot **v2**, stesso slot civile e backup dell'originale prima della prima scrittura; nessuna produzione offline o uso di partite personali nei test.
- Cataloghi storici v4/v5/v6/v7 preservati; **`CreateForgeForBenchmarks()` fissa v8** per la compatibilità. Il benchmark M5.1c e l'esperimento separato restano su v7. Non aggiungere contenuti a un ID di regole già persistito.

## Interfaccia acquisita

Zecca e Monete hanno icone originali 8×8, ricette e diagnosi per Oro, Carbone, personale e capienza. La tabella mantiene ordine alfabetico italiano, saldi e stima del riempimento. La v38 resta acquisita: effetti di case/Magazzino nel medesimo template di costi, produzione e consumi, monospace normale 12 px e icone 14 px. Effetti per edificio anche negli ordini multipli.

## Decisioni già ricevute

Non richiedere nuovamente queste scelte:

- Prima campagna da 22 tipi, vittoria col possesso di tutti, edifici iniziali validi; Castello conclusivo dopo una conquista obbligatoria. Parametri della missione ancora aperti.
- S1 e configurazioni A Ferro/Oro/Forgia/Zecca approvate. Spade consumate dal futuro arruolamento; Monete per pagamenti espliciti. Quantità per recluta e nave da scegliere nelle fasi M5.3/M5.4.
- Grafica retro monospace, icone, card e hover/selezione ocra approvati. Demolizione **Rinviata**, Carne fuori dalla prima campagna.

## Prossimo lavoro

1. **M5.2c:** confrontare nel catalogo corrente produzione a fasi con una miniera e parallela con due, acquisti reali e contabilità per secondo. Riprodurre l'obiettivo 240 Spade/360 Monete, capienze e replay interi/frazionari, su 1.800 secondi e una cronologia più lunga; aggiungere una strategia da nuova partita che integri entrambe le filiere. Conservare risultati nel validatore senza cambiare i prezzi approvati.
2. L'[esperimento isolato](../experiments/forge-mint-probe/README.md) eseguito nel precedente incremento conferma **241/122 s** nel checkpoint sintetico v7, sei profili A/B e sei guardie. Non è stato rieseguito in v39 e non sostituisce M5.2c nel gioco corrente.
3. Poi M5.3 popolazione militare/Caserma, M5.4 Cantiere/flotta, missione, conquista e Castello. Definire i contratti a valle mancanti prima del codice.

Restano aperti DPI fisici, altre piattaforme e accessibilità. Gli obiettivi di circa 8 ore efficienti / 32+ ore lente riguardano la campagna completa e non sono ancora dimostrati. [Guida di sviluppo](DEVELOPMENT.md).
