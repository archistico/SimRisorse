# Handoff

Ultimo aggiornamento: 2026-10-27. Questo documento descrive il punto operativo da cui continuare; visione e regole complete sono nel [game design](GAME_DESIGN.md).

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
