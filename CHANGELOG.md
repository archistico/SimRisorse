# Changelog

Questo file registra le modifiche realizzate. Le attività previste appartengono alla [roadmap](docs/ROADMAP.md). Non è stata ancora pubblicata una versione della nuova applicazione.

## [Unreleased]

### 2026-10-27 — Prova industriale A/B separata dal gioco

- Aggiunto [IndustryProbe](experiments/industry-probe/README.md), eseguibile senza nuove dipendenze esterne, fuori dalla soluzione di produzione. Usa Core/Application e una colonia sintetica per misurare ipotesi A/B su ferro e oro, con acquisti in blocco o progressivi.
- Eseguiti otto confronti con riconciliazione di 3.601 secondi ciascuno, ripetuti con passi da 1 s e 250+750 ms; rapporti e snapshot identici, saturazione e ripresa dopo espansione pagata del deposito verificate. Primo ferro progressivo A/B: **452/677 s**; primo oro: **852/1.277 s**. La politica progressiva evita il Magazzino intermedio del profilo B con budget intero.
- Build sperimentale senza avvisi/errori; nuovo `validate.cmd` completo superato con **177/177 test**, simulazioni e smoke nativi per entrambi gli scenari. Ipotesi economiche, versioni di regole sperimentali e capacità restano limitate alla prova: nessuna nuova regola nel desktop o migrazione dei salvataggi.

### 2026-10-27 — D0.1: scelto il catalogo della prima campagna

- Preparata la [scheda concreta della prima campagna](docs/design/FIRST_CAMPAIGN_PROPOSAL.md) con corrispondenza ai 43 riferimenti, ID presenti/candidati, percorso funzionale e sette casi proposti di vittoria/non vittoria.
- **L'utente ha scelto i 22 tipi proposti:** i 12 presenti più cinque tipi di estrazione/fusione, Forgia, Zecca, Caserma, Cantiere e Castello. Gli altri 21 riferimenti sono fuori dalla prima campagna. Registrata D0.1 nel game design, dossier D0 e handoff; vittoria, isole, costi, ricette e salvataggi restano decisioni separate. Nessun contenuto M5 implementato.

### 2026-10-27 — Consolidamento dei risultati M4 dopo il gate Windows

- Eseguito `scripts/report-m4.ps1` sui quattro benchmark reali e confrontata l'uscita con una lettura delle copie isolate. Conservati localmente i cinque input JSON con impronte SHA-256 e il rapporto generato, senza modificare le simulazioni.
- Ispezionati i nove render e i conteggi dei quattro TRX. Creato il [rapporto delle misure M4](docs/validation/M4_MEASURED_RESULTS.md): confronto abitazioni v4/v5, costi e carenze Pane a 220/320 abitanti, revisione visiva e limiti della verifica. Aggiornati i punti di ripresa; approvazione estetica, Carne/demolizione e decisioni M5 restano aperte.

### 2026-10-27 — Correzione del gate Windows: palette PixelGlyph

- Sostituiti i pennelli statici mutabili di `PixelGlyph` con `ImmutableSolidColorBrush`, vincolando la palette a `IImmutableBrush`. Il controllo del catalogo può inizializzare il tipo fuori dal thread grafico: i pennelli condivisi ora si possono usare nel rendering senza l'eccezione Avalonia di accesso da un thread diverso, che causava 21 errori Desktop.
- **Verificato con `.\validate.cmd` su Windows**: build Release con zero avvisi/errori, **177/177 test superati**, controlli cataloghi, simulazioni e quattro benchmark completati, nove render generati e prove native di salvataggio/caricamento/ripresa superate per entrambi gli scenari. Report locale `.local-cache/m4/validation.json`, esecuzione `98d961210607483a8afe8d708039efab`. Nessun cambiamento alle regole o ai salvataggi.

### 2026-10-27 — v29 / M4.15: studio navale M5.4 (solo progettazione)

- Creato il [dossier M5.4 su Cantiere navale, flotta e trasporto](docs/design/M5_4_NAVAL_PREFEASIBILITY.md), distinguendo quanto appare nei diagrammi da ciò che resta da decidere: costo del Cantiere e delle navi, uso delle Monete, capacità, equipaggio, riutilizzo, ritorni e commercio coloniale.
- Confrontati tre modelli di nave (monouso, entità persistente, capacità astratta); formalizzati i controlli sul **primo viaggio raggiungibile**, gli acquisti atomici, l'identità delle navi, le future prenotazioni senza doppio impiego e i test di salvataggio. Nessuna proposta o costo è stato approvato.
- Roadmap, scheda D0, game design, handoff, README e verifica M4 allineati: **M5.4a/b/c restano futuri**, distinti dalle missioni M5.5 e dalla conquista M5.6. Nessuna modifica a codice, test, benchmark, UI, risorse, regole `civil-growth-5` o SQLite. Gate confermato dall'utente fino a **v24** (174 xUnit attesi); v25–v29 **pendenti su Windows** (177 xUnit attesi).

### 2026-10-27 — v28 / M4.14: scheda operativa D0 e correzione delle baseline M5

- Creata la [scheda decisionale D0](docs/design/D0_DECISION_BOARD.md): otto domande, preferenze **non approvate**, ordine delle dipendenze, verifiche richieste, politica S1/S2 di salvataggio e criteri per iniziare M5.1a.
- Corretti gli studi [M5.1](docs/design/M5_1_INDUSTRY_PREFEASIBILITY.md), [M5.2](docs/design/M5_2_FORGE_MINT_PREFEASIBILITY.md) e [M5.3](docs/design/M5_3_ARMY_PREFEASIBILITY.md), che in vari punti citavano erroneamente `civil-growth-4` come base eseguibile: la versione **corrente è `civil-growth-5`**; l'eventuale `civil-growth-6` è **solo una proposta** subordinata alla scelta S1. I test futuri devono includere anche la conversione da `-5`.
- Allineati dossier D0, roadmap, handoff, README e verifica M4. **Nessuna modifica** a motore, GUI, test, benchmark, cataloghi, SQLite o salvataggi. Ultimo gate confermato dall'utente: **v24** (174 xUnit attesi); v25–v28 **pendenti** (177 xUnit attesi), senza SDK .NET/PowerShell in questo ambiente.

### 2026-10-27 — v27 / M4.13: audit degli incentivi abitativi e allineamento del gate D0

- Redatto l'[audit matematico delle abitazioni](docs/scenarios/HOUSING_INCENTIVE_AUDIT.md) a partire da `HousingGrowthRules`, `CivilGrowthScenario` e dalla razione `PopulationPlanner`, senza modificare il gioco. Il limite **2×** richiede sette unità di bonus equivalenti: le quattro combinazioni a minimo costo abitativo spendono tutte **560 legno lavorato e 280 pietra**, ma offrono **64/80/96/112 posti**.
- Quantificati per ciascuna combinazione Cibo e Pane necessari a piena capienza, numero nominale di filiere e lavoratori alimentari; distinta la convenienza **per posto** dalla spesa di sblocco della filiera Pane e dai rischi di carenza. A 112 residenti una filiera Cibo da 1/s non copre più il consumo continuo.
- Aggiornato il [dossier D0](docs/design/PRE_M5_DECISION_GATE.md): stato reale v24–v27, versione attiva `civil-growth-5` e tetti **ideali** di 968 abitanti in 8 ore / 3.848 in 32 ore a bonus 2× fin dall'inizio, anziché applicare impropriamente i precedenti limiti a 60 s dell'algoritmo storico.
- Roadmap, handoff e documenti M4 allineati. **Nessun cambiamento al Core, alla GUI 8-bit, ai benchmark, agli schemi SQLite o ai salvataggi** e **nessun nuovo test xUnit**: **177 casi attesi** come per v25/v26, non eseguiti qui. v24 resta l'ultimo gate riferito superato dall'utente; D0 e i ritocchi economici restano decisioni aperte.

### 2026-10-27 — v26 / M4.12: rapporto quantitativo M4, senza toccare le regole

- **v25/M4.11 resta da verificare su Windows** (177 test xUnit attesi). L'utente non può eseguire ora i test: nessun ulteriore cambiamento alla GUI non ancora osservata o alle regole di gioco.
- Nuovo `scripts/report-m4.ps1`: legge i **quattro JSON** prodotti dalla validazione e produce una sintesi Markdown di tempi 48/100/136 abitanti con/senza bonus, confronti del Pane a 220/320, carenze, scorte, produzione, consumo e investimenti aggiuntivi.
- Controlla versione dello scenario, durata, ID delle strategie, duplicazioni e **riconciliazione di ogni secondo**; segnala assenza di report anziché usare valori teorici come risultati osservati. Non avvia .NET e non legge o modifica salvataggi personali.
- Di default stampa il resoconto. Un file si crea soltanto con `-OutputPath`, senza sovrascrivere file esistenti salvo `-Force` esplicito. Lo script è codificato UTF-8 con BOM per PowerShell Windows.
- Creato il [manuale di lettura dei benchmark](docs/validation/M4_BENCHMARK_READER.md), aggiornati README, roadmap, handoff e report M4. **Nessun test Windows/PowerShell eseguito qui**, né nuovo test xUnit; **177 casi previsti** come in v25. Nessuna meccanica M5 è approvata.

### 2026-10-27 — v25 / M4.11: prima schermata retro 8-bit (gate Windows in attesa)

- Il **gate v24/M4.10 è riferito superato dall'utente** (174 casi xUnit attesi, TRX/benchmark grezzi non allegati). Le regole `civil-growth-5` restano il riferimento della nuova GUI.
- Introdotto `PixelGlyph`, controllo Avalonia autonomo con **21 sprite vettoriali a celle 8×8** progettati per 12 edifici e 9 risorse. Identificatori di catalogo → disegni originali, senza nuovi pacchetti né asset esterni.
- Integrati i pittogrammi nell'elenco strutture, nelle schede delle scorte e nella tabella Risorse. Prima palette retro blu notte/crema/azzurro e pannelli con bordi netti; numeri, testi, diagnosi e comandi restano visibili.
- Mantenuto lo spessore fisso del bordo del pulsante **Costruisci** per evitare la regressione dell'hover; mantenuta la diagnosi dei prerequisiti sopra ai comandi senza scorrimento.
- Aggiunti tre casi Desktop attesi: copertura di tutti gli ID civili e verifica delle icone e diagnosi alle due risoluzioni. **177 test xUnit attesi**, non eseguiti qui senza SDK .NET; necessari `validate.cmd` e controllo visivo dei PNG.
- **Nessuna modifica a prezzi, ricette, regole, salvataggi, backup o benchmark.** È un prototipo da valutare (D0.7), non la GUI finale.

### 2026-10-27 — v24 / M4.10: accelerazione abitativa `civil-growth-5` (nuovo gate Windows in attesa)

- Ogni Casa piccola oltre la prima aggiunge **15%** alla velocità di arrivo di un residente; ogni Casa grande aggiunge **30%**. Bonus cumulativi e limitati a **2×** (60→30 secondi a velocità massima). Nessun bonus per la sola Casa piccola iniziale; cibo, Pane necessario e capienza continuano a vincolare gli arrivi.
- Introdotto accumulatore deterministico in ventesimi di secondo: cambi di velocità, pause, carenze e passi temporali suddivisi non perdono la frazione. Forecast e GUI mostrano velocità, progresso e stima dei secondi all'arrivo.
- Conversione dichiarata `civil-growth-4 → civil-growth-5`, con backup dell'originale; formato snapshot/SQLite **v2** invariato. La tabella opzionale `population_growth_fraction` salva il resto: vecchi file v2 senza tabella restano leggibili con frazione zero.
- Conservati i benchmark quantitativi M4 a regole `civil-growth-4`, separati dal nuovo `--benchmark-housing-accelerated`. Aggiunti **7 test xUnit** in Application, Infrastructure e Desktop (totale **174 attesi**) e una prova di crescita nel validatore Windows.
- **Codice implementato ma non compilato né verificato con .NET in questo ambiente**. Il gate v23 è riferito superato dall'utente; ricette, costi, catalogo, decisioni M5 ancora aperte e futuro stile pixel art restano invariati.


### 2026-10-27 — Checkpoint v23: hover senza salto di layout e studio M5.3

- L'utente conferma **«test passati» per v22** (GUI compatta, 166 casi xUnit previsti); i report grezzi non sono disponibili in questa sessione. Ha segnalato che passando su **Costruisci** gli elementi precedenti si spostano verso l'alto.
- Causa individuata nel codice: `Button.primary.build:pointerover` applicava `BorderThickness=2`, mentre il disabilitato aveva `1`. Il diverso bordo variava l'ingombro del controllo all'attivazione dell'hover. Ora `Button.primary.build` mantiene **BorderThickness=1 in ogni stato**; hover/pressione/disabilitato modificano colore e bordo ma non spessore. Aggiunto un nome al testo del costo per verificarne la posizione.
- Rafforzato il test Desktop: confronto di posizione e dimensioni di **selettore quantità, costo e pulsante** prima/dopo hover e dopo esaurimento dei materiali, sia a **1040×740** sia a **1240×850**. Il precedente caso singolo è ora una teoria da due: **167 casi xUnit complessivi attesi** (+1 rispetto a v22), **non eseguiti qui** per assenza di .NET.
- Preparato il [dossier M5.3](docs/design/M5_3_ARMY_PREFEASIBILITY.md): Caserma, reclute, soldati, conservazione della popolazione, alternative di addestramento, test e decisioni D0.4/D0.8. **Solo prefattibilità:** non sono approvati costi, tempi o perdite e non è stato implementato alcun contenuto militare. Regole `civil-growth-4`, salvataggi e benchmark restano invariati.

### 2026-10-27 — Checkpoint v22: dettaglio Edifici compatto e stati distinti di Costruisci

- L'utente conferma **«test passati» per v21**: il codice incluso corrisponde allo stress M4.8 realizzato in v18 (163 casi xUnit attesi, nove render e tre benchmark). La conferma è registrata come esito dell'utente; non sono disponibili TRX, PNG o i JSON con i valori numerici da analizzare.
- Nella GUI provvisoria, `Costruisci +1` distingue ora lo stato **normale verde scuro**, **hover verde più chiaro con bordo**, **pressione verde intenso** e **disabilitato grigio**. Il motivo di blocco e i prerequisiti della struttura selezionata sono spostati nella parte **fissa sopra i comandi**, così non richiedono lo scorrimento del dettaglio.
- Ridotti margini e spaziature di sidebar, testata, schede riassuntive, elenco edifici, pannello dettagli, tabella Risorse e barra inferiore per usare meglio le finestre da 1040×740 e 1240×850. La destinazione grafica resta **pixel art retro 8-bit essenziale**; nessun intervento alla logica di produzione, ai prezzi, alle regole `civil-growth-4` o al salvataggio.
- Aggiunti **tre casi Desktop**: due geometrie per verificare che la diagnosi sia interamente visibile senza scroll e un controllo di colori hover/disabilitato. **166 casi xUnit attesi, non eseguiti** qui perché manca .NET; necessaria validazione Windows e ispezione del render compatto. Il gate M4.8 precedente rimane confermato dall'utente, mentre quello **GUI v22 è in attesa**.

### 2026-10-27 — Checkpoint v21 solo documentale: studio M5.2 su Forgia e Zecca

- Creato il [dossier M5.2](docs/design/M5_2_FORGE_MINT_PREFEASIBILITY.md) su Forgia (Ferro+Carbone→Spade) e Zecca (Oro+Carbone→Monete), con ipotesi numerica A e alternativa B a costi maggiorati. Le fonti mostrano le relazioni, non valori numerici; **nessun costo, ricetta o prerequisito è approvato**.
- Quantificata la contesa sul Carbone per le proposte M5.1+M5.2: quattro trasformatori chiederebbero 4 Carbone/s contro 2/s di una sola miniera; considerati lavoro per fasi, accumulo preventivo di metalli e seconda miniera, con somme dei materiali e del personale ricontrollabili. Non sono risultati di simulazione .NET.
- Esplicitata la decisione **D0.8**: Spade consumabili o equipaggiamento persistente; Monete come scorta usata da specifici comandi o valuta generale; ruolo futuro delle fonti coloniali. Preparati sette profili di test futuri, senza aggiungere codice al gioco.
- Allineati dossier M5.1, roadmap, gate D0, game design, handoff e README. **Versione eseguibile invariata da v18, ancora da validare in M4.8; ultimo gate riferito superato dall'utente v17, 163 test xUnit attesi.**

### 2026-10-27 — Checkpoint v20 solo documentale: prefattibilità industriale M5.1

- Preparato lo [studio industriale](docs/design/M5_1_INDUSTRY_PREFEASIBILITY.md) su **M5.1a ferro/carbone**, **M5.1b oro**, **M5.1c misura di scala e contesa**. Il riferimento originale documenta le relazioni fra le filiere, non valori numerici.
- Proposto un set A *non approvato* di cinque edifici con costi, ricette, lavoratori e prerequisiti: primo ferro teorico a **450 legno lavorato / 320 pietra**, tre addetti, **1 ferro/s**; set B con costi 1,5× per confrontare la progressione. Documentati saturazione del carbone, deposito, output differito e bilanciamento con Cibo/Pane.
- Esplicitate due alternative di migrazione da M4 a M5 (**estendere lo scenario civile** o **creare una campagna con importazione dichiarata**), più test richiesti per Core, proiezioni, SQLite, UI e benchmark. **Nessuna decisione è approvata o implementata**: il codice e il validatore sono identici alla v18, il gate M4.8 resta in attesa.
- Aggiunta allo [stress M4.8](docs/scenarios/BREAD_HIGH_DENSITY_BENCHMARK.md) una dimostrazione di capacità senza .NET: in 1.800 s la domanda minima è **8.424 Pane**, la snella può fornirne al massimo **8.200** inclusa la scorta iniziale; manca quindi almeno **224 Pane** per un rifornimento ininterrotto. Non si deducono da questo numero i secondi di blocco o la popolazione finale effettiva.
- Allineati roadmap, documenti D0, handoff, game design, salvataggi e README al checkpoint documentale. La GUI finale resta pixel art retro 8-bit essenziale, senza posizionamento sulla mappa.

### 2026-10-27 — Checkpoint v19 solo documentale: preparazione del gate D0 prima di M5

- Preparato il [dossier decisionale pre-M5](docs/design/PRE_M5_DECISION_GATE.md) con inventario **di tutti i 43 tipi strutturali dei riferimenti**, mappatura dei **12 equivalenti presenti** nel gioco e classificazione dei **31** da valutare. Non sono stati aggiunti edifici al catalogo.
- Confrontate alternative per vittoria, ruolo delle isole e rapporto fra popolazione civile/militare; proposti pacchetti D0.1–D0.7, criteri di raggiungibilità e vincolo massimo aritmetico di **488 abitanti dopo otto ore** senza blocchi. Ogni preferenza resta una **proposta**, non un requisito approvato.
- Allineati roadmap, game design, README, direzione GUI 8-bit, studio Carne e handoff al gate **v17 riferito superato**, con nuovo stress **v18 ancora da validare**. Nessun file del motore, codice, test, script di validazione, SQLite, GUI o salvataggi modificato rispetto a v18.

### 2026-10-27 — M4.8: stress Pane ad alta densità e conferma del gate v17

- L'utente ha riferito superati i test e la validazione del checkpoint **v17**, comprensivi del confronto Pane `lean-chain` e della diagnostica M4.6/M4.7 (**163 casi xUnit attesi**). Non sono disponibili i report TRX/JSON grezzi: il superamento è registrato come conferma dell'utente, non come prova rieseguita localmente.
- Aggiunto `--benchmark-bread-high-density`: confronto autonomo delle quattro strategie da **320 abitanti**, 360 posti, quattro filiere Cibo, 100 Pane di riserva e 1.800 secondi, con tempi per 340/350 abitanti e rendicontazione delle scorte. Il benchmark M4.7 da 220 abitanti rimane disponibile e invariato nelle ipotesi.
- Estesa la validazione generale con il terzo JSON `bread-high-density-benchmark.json`, la riconciliazione delle 4 × 1.800 transizioni, i prezzi e il personale, e le ipotesi da verificare sull'ordine di crescita delle filiere. Documentati scenari, percorso dei report, roadmap e handoff.
- **Nuovo gate M4.8 implementato ma non compilato o eseguito con .NET in questo ambiente.** Restano 163 casi xUnit attesi. Nessuna modifica alle regole `civil-growth-4`, al motore, ai cataloghi, alla GUI, a SQLite o ai salvataggi.

### 2026-10-27 — M4.7: quarto benchmark Pane con filiera snella

- Esteso `--benchmark-bread-scale` con `lean-chain`: potenziamento immediato di **1 Fattoria + 1 Pozzo + 1 Mulino + 1 Forno**, acquistati e dotati di personale con i comandi del Core; +4 lavoratori, **360 legno lavorato e 250 pietra**, contro +5/420/280 del potenziamento completo.
- I quattro scenari partono dalla stessa colonia sintetica da 220 abitanti e affrontano 1.800 passi ciascuno; il report misura ora secondi-lavoratore per Fattorie/Pozzi/Mulini/Forni, Pane realmente prodotto e consumato e scorte intermedie finali. Contabilità del Pane e previsione economica vengono riconciliate con lo stato effettivo.
- Esteso `scripts/validate.ps1` a **quattro** politiche con controlli su prezzi, addetti e relazione tra crescita delle strategie; aggiornati `--help`, [studio del benchmark](docs/scenarios/BREAD_SCALE_BENCHMARK.md), [modello teorico](docs/scenarios/BREAD_CHAIN_CAPACITY.md), [roadmap](docs/ROADMAP.md), [handoff](docs/HANDOFF.md) e [gate M4](docs/validation/M4_CIVIL_GROWTH.md).
- **Implementato, non ancora compilato o eseguito con .NET in questo ambiente.** Restano 163 casi xUnit attesi; il nuovo benchmark non aggiunge un test xUnit ma estende il validatore. Nessuna modifica a `civil-growth-4`, catalogo, SQLite, GUI o salvataggi. La capacità teorica di **4,5 Pane/s** non è dichiarata produttività realmente osservata.

### 2026-10-27 — Documentazione: roadmap operativa e formato dei resoconti

- Riorganizzata la [roadmap](docs/ROADMAP.md) per distinguere M0–M3 completate, M4.1–M4.5 confermate, diagnostica M4.6 da validare, benchmark della filiera snella M4.7 da sviluppare e criteri di chiusura M4.8.
- Pianificato il gate di design **prima di M5** per catalogo, condizione di vittoria, conquiste, popolazione militare e tempo; dettagliate M5 industria/esercito/isole, M6 vittoria/bilanciamento e M7 qualità/rilascio. Confermata l'introduzione graduale della **GUI pixel art retro 8-bit essenziale** dopo la stabilizzazione M4.
- Allineati [game design](docs/GAME_DESIGN.md), [handoff](docs/HANDOFF.md), [convenzioni di sviluppo](docs/DEVELOPMENT.md), [direzione grafica](docs/PIXEL_ART_UI.md) e README. Ogni resoconto futuro indicherà stato, fatto, in corso, prossimi passi e verifiche aperte.
- **Solo documentazione**: nessuna modifica a sorgenti, regole `civil-growth-4`, catalogo, SQLite, asset o test. Il gate v14–v15 resta **163 casi xUnit attesi, non ancora verificati**.

### 2026-10-27 — M4: limite produttivo dell'intera filiera Pane

- Aggiunto un proiettore Application che segue a ritroso le ricette univoche di Pane, Farina, Acqua e Grano: per ogni anello espone fabbisogno medio, produzione nominale, deficit, addetti presenti e minimi teorici. Non presenta una stima in cataloghi con produttori alternativi, coprodotti o cicli, evitando suggerimenti non fondati.
- `GameView.BreadSupply.ChainCapacity` contiene l'**equivalente medio teorico in Pane/s** della filiera e gli anelli limitanti; resta distinto dalla produzione effettiva prevista nel prossimo secondo.
- Rafforzato il caso Application da 220 abitanti: una filiera completa (3 Pane/s), due Forni senza seconda filiera (ancora 3), Grano limitante (1,5), due filiere complete (6 nominali, ma zero Pane effettivamente previsto se mancano input). I test precedenti non sono stati modificati nel loro intento.
- Documentata la proposta di **filiera snella** (+4 lavoratori, 360 legno lavorato e 250 pietra, contro +5, 420 e 280 per il raddoppio): il vantaggio è soltanto un limite teorico, da confrontare poi con simulazioni reali. Nessun costo o meccanismo modificato.
- **163 casi xUnit attesi**, stesso numero del pacchetto precedente ma con nuove asserzioni; gate Windows ancora da eseguire. Versione `civil-growth-4`, SQLite e GUI attuale invariati; futura veste retro 8-bit predisposta a visualizzare gli anelli separatamente.

### 2026-10-27 — M4: deficit quantitativo Pane e capacità dei Forni

- L'utente ha confermato **test e validazione superati** anche per il benchmark di scala Pane a 220 abitanti del pacchetto precedente; non sono disponibili i relativi report grezzi.
- `GameView.BreadSupply` distingue ora domanda Pane, produzione nominale degli addetti, produzione prevista nel prossimo secondo, differenza nominale e numero minimo/addizionale di addetti quando esiste una sola ricetta per il Pane. Dati pronti per indicatori compatti nella futura GUI retro 8-bit.
- La sidebar indica, quando i Forni sono sottodimensionati, il deficit in Pane/s e il numero addizionale di addetti. La carenza di ingredienti continua a essere distinta dalla capacità nominale; nessun suggerimento acquista edifici in automatico.
- Aggiunto un test Application a 220 abitanti (212 avanzati): un Forno da 3 Pane/s contro domanda 3,180; due Forni senza ingredienti restano fermi. Rafforzati i test esistenti e il controllo di ingombro della sidebar a 1040×740. **163 casi xUnit attesi, nuovo gate da eseguire**; regole, formato SQLite e sprite invariati.

### 2026-10-27 — M4: nuova prova di scala Pane dopo gate utente superato

- Registrata la conferma dell'utente sul superamento di test e validazione della precedente versione (162 casi attesi); i report grezzi non sono stati allegati a questa sessione.
- Aggiunto `--benchmark-bread-scale`: partendo da uno snapshot sintetico a 220 abitanti, confronta una filiera Pane completa, due filiere immediate e la seconda costruita dopo 15 minuti. Le nuove costruzioni passano dai comandi reali, con costi e personale effettivi.
- Controlla secondo per secondo ricette, razioni, progresso e scorte; produce un riepilogo di popolazione, carenze, produttività e spesa aggiuntiva. Il validatore salva un JSON dedicato ed esegue controlli di coerenza e confronto fra le politiche.
- Nessuna modifica a `civil-growth-4`, ai salvataggi o alla GUI 8-bit pianificata. **Il nuovo benchmark non è ancora stato eseguito** in un ambiente .NET; le prove pregresse rimangono confermate dall'utente.

### 2026-10-27 — M4: proiezione della sostenibilità Pane riutilizzabile dalla GUI 8-bit

- Spostata la diagnosi del prossimo secondo da `MainViewModel` a un proiettore Application che espone `GameView.BreadSupply`, stato tipizzato e autonomia teorica della scorta. L'interfaccia Avalonia usa il modello senza calcolare produttività o confrontare input.
- Migliorata l'informazione prima del primo abitante avanzato: l'avviso sulla necessità del Pane resta visibile anche quando la crescita segnala prioritariamente **mancanza di Cibo**. Consumi, arrivi e progresso rimangono invariati.
- Aggiunti due test Application sui blocchi simultanei e sulle differenze fra carenza input, Forno operativo e deposito pieno; tre test Desktop controllano ora anche gli stati tipizzati.
- Nessuna variazione alle regole `civil-growth-4`, ai salvataggi o agli asset grafici. **162 casi xUnit attesi, non eseguiti** senza SDK .NET; ultimo checkpoint confermato: 140 test superati.

### 2026-10-27 — M4: progettazione della demolizione e direzione grafica 8-bit

- Registrata la scelta esplicita dell'utente: futura GUI **pixel art retro 8-bit essenziale**, mantenendo edifici in elenchi e schede. Aggiunti [direzione grafica](docs/PIXEL_ART_UI.md) e [ADR 0012](docs/adr/0012-retro-8bit-desktop-visual-language.md), senza modificare Desktop.
- Preparata la [valutazione della demolizione Q-08](docs/scenarios/DEMOLITION_M4_REVIEW.md): rimborsi candidati 0/25/50%, scenari quantitativi e invarianti per abitanti, lavoratori, dipendenze e magazzini. Il 25% è solo proposto: nessuna demolizione abilitata.
- `civil-growth-4`, costo delle Case grandi, ricetta Pane, salvataggi e suite xUnit invariati. **Gate corrente: 160 casi attesi, non eseguiti**; ultimo checkpoint confermato: 140 test superati.

### 2026-10-27 — M4: ripresa da carenze di Pane e Cibo

- Corretto il caso iniziale della diagnosi UI: se una Casa grande offre posti ma manca ancora Pane per il **primo** residente avanzato, il messaggio operativo viene mostrato anche con zero residenti avanzati. Prima la diagnosi rimaneva nascosta proprio durante lo sblocco.
- Aggiunto un test Desktop per la sequenza 16→17 abitanti con 59 secondi di crescita già accumulati e Pane inizialmente a zero: Pane prodotto al primo secondo, arrivo al secondo, consumo del nuovo residente soltanto dal terzo.
- Aggiunto un test Application con Cibo iniziale a zero, Pesce già disponibile, un Deposito alimentare attivo e Pane in scorta: la carenza non consuma Pane, il Cibo prodotto diventa utilizzabile dal passo successivo e il progresso parziale viene conservato.
- Nessuna modifica alle regole, ai costi, alle ricette o al formato del salvataggio. **160 test xUnit attesi** (2 nuovi casi), **non eseguiti** con .NET. Ultimo checkpoint confermato: 140.

### 2026-10-27 — M4: diagnosi della sostenibilità del Pane nell'interfaccia

- Nella sezione Crescita, quando sono presenti residenti avanzati, la UI distingue capacità nominale insufficiente dei Forni, input momentaneamente insufficienti, deposito quasi pieno e produzione disponibile per il prossimo secondo.
- Mostrata l'autonomia della scorta di Pane **senza nuove forniture, al consumo corrente**; non è una previsione dell'esaurimento reale con produzione attiva né una garanzia di mantenimento futuro.
- Rafforzato il test Desktop della razione avanzata e aggiunto un nuovo caso che verifica un Forno fermo per Pane pieno: il saldo momentaneo negativo non va presentato come capacità nominale insufficiente. La vista compatta richiede ancora ispezione Windows.
- Predisposto un nono render, `civil-advanced-bread-compact.png`, a 1040×740, con controllo del posizionamento della diagnosi Pane. Il validatore verifica che tutti e nove i PNG siano stati generati nella **cartella della specifica esecuzione**, escludendo falsi esiti dovuti a immagini vecchie.
- Nessun cambiamento alle regole, alle ricette, alle altre risorse o al formato del salvataggio. **158 casi xUnit attesi**, ancora non eseguiti con .NET; ultimo checkpoint confermato: 140.

### 2026-10-27 — M4: riconciliazione dei bilanci e soglia produttiva del Pane

- Il confronto `--benchmark-housing` verifica ora a ogni secondo, per le tre strategie, l'equivalenza fra consumi e ricette previsti e scorte effettive, nonché arrivi e progresso della crescita. Registra `ReconciledSeconds`; il validatore richiede 28.800 secondi riconciliati per strategia.
- Aggiunto un test Core oltre 200 residenti avanzati: 1 Forno, 2 Forni dall'inizio e secondo Forno dopo 15 minuti. Il rinforzo produttivo deve migliorare la progressione, senza recuperare retroattivamente il tempo perso.
- Un modello aritmetico indipendente da .NET prevede 246, 250 e 249 abitanti dopo 30 minuti a partire da 220, nei tre casi rispettivi. Questi numeri **non sono stati verificati con .NET**. **157 test xUnit attesi**; ultimo checkpoint realmente verificato: 140. Regole, costi e salvataggi invariati.

### 2026-10-27 — M4: riproducibilità della progressione avanzata (gate .NET in attesa)

- Aggiunti due test Application di riproduzione fino a due ore: cronologia con chiamate da un secondo contro intervalli disomogenei, seconda Casa grande, confronto esatto di edifici, scorte, tempo, abitanti e progresso della crescita. Un salvataggio intermedio a 879,5 secondi verifica la ripresa dopo pausa e l'arrivo con gli ultimi 0,5 secondi.
- Aggiunto un test Infrastructure con SQLite reale per una colonia già fornita di Forno e Casa grande: salvataggio a 879,5 secondi, ricaricamento in pausa, ripresa e confronto con la partita mai interrotta.
- Nessuna modifica a formule, costi, versioni delle regole o formato di salvataggio. Conteggio xUnit **atteso: 156 casi** (inclusi i 3 nuovi), **non eseguiti** per assenza di SDK .NET. Checkpoint confermato: 140 test.

### 2026-10-27 — M4: confronto quantitativo di tre strategie (esecuzione .NET in attesa)

- Valutata la filiera della Carne sulla base dei riferimenti originali: aggiungerla come semplice duplicato del Pesce non risolve un vincolo del gioco; [documentate alternative e rinvio possibile a M5](docs/scenarios/MEAT_DESIGN_REVIEW.md), senza cambiare il catalogo.
- Eseguito un controllo aritmetico indipendente, esterno a .NET, sulle tre cronologie: le soglie e i materiali attesi coincidono. Il Magazzino aumenta la scorta di Pane senza anticipare gli arrivi nelle politiche considerate; il risultato rimane **preliminare** e non sostituisce l'esecuzione del motore.
- Rafforzati i controlli del benchmark sulla diagnostica di attività/capacità del Forno, un test Core sul mancato consumo di Pane quando manca Cibo e la prova di conversione SQLite di un checkpoint storico con Forno e lavoratori già presenti. Il totale xUnit atteso sale a **153**, ancora non eseguiti.
- Preparato `--benchmark-housing` nel simulatore: confronta Case piccole, Case grandi standard e Case grandi anticipate con secondo Pescatore e Magazzino, su otto ore simulate. Registra tempi per 48/100/136 abitanti, spese, inattività del Forno, carenze e posti occupati.
- La validazione ora prevede anche un confronto JSON e controlli sulle soglie teoriche e i materiali; i tre profili sono descritti in [HOUSING_BENCHMARK](docs/scenarios/HOUSING_BENCHMARK.md). **Non eseguito:** ultimo checkpoint confermato ancora 140 test, nessuna modifica alle regole `civil-growth-4` o ai salvataggi.
- Il confronto con la Casa piccola aggiuntiva necessaria alla filiera nei profili di riferimento mostra un pareggio più prudente: a quattro Case grandi, risparmio previsto di 140 legno lavorato e parità di pietra, con otto posti aggiuntivi. Non è una conclusione sull'ottimalità.

### 2026-10-27 — M4: Casa grande, razioni Pane e regole `civil-growth-4` (gate Windows in attesa)

- Approvato il modello quantitativo: Casa grande (+32 posti, costo 160 legno lavorato + 80 pietra, Forno prerequisito); il Forno produce 3 Pane/s, risorsa distinta da Cibo, con capacità iniziale 100 (+500 per Magazzino).
- Consumano tutti 0,010 Cibo/abitante/s; chi supera i posti delle Case piccole richiede inoltre 0,015 Pane/abitante/s. Scorte insufficienti sospendono la crescita, senza rimuovere abitanti o resettare i progressi. Core, viste e UI mostrano residenti avanzati, fabbisogno e carenze.
- Aggiunta conversione dichiarata `civil-growth-3 → civil-growth-4`, concatenabile da `-1/-2`: nuove Case grandi a zero, nuovo Pane a zero, nessuna trasformazione del Cibo pregresso. Il formato SQLite/snapshot resta v2 e il primo salvataggio conserva la copia dell'originale.
- Aggiunti test per razioni, soglia 16→17 residenti, recupero dopo carenze, capacità del Pane, interfaccia e SQLite; nuove cronologie `bread-start`, `advanced` a 880 secondi e `advanced` a 8 ore nello script di validazione. Aggiornati [scenario](docs/scenarios/CIVIL_GROWTH.md), [ADR 0011](docs/adr/0011-advanced-housing-and-bread-rations.md), formato e handoff.
- **Non ancora verificati con build, test o render Windows:** l'ambiente corrente non dispone di .NET. Ultimo checkpoint confermato: **140 test superati**.

### 2026-10-27 — Studio M4: progressione abitativa e pane (proposta poi approvata)

- Preparato `docs/scenarios/ADVANCED_HOUSING_PROPOSAL.md`: confronto fra prerequisito semplice e risorsa Pane distinta, ipotesi Casa grande da 32 posti a costo 160/80, consumo aggiuntivo 0,015 Pane/abitante avanzato/s, capacità Pane 100, punto di pareggio dei materiali dalla quarta Casa grande.
- Documentati impatto su Core, UI e conversioni, rischi di scorta/produzione e test necessari. Nello studio iniziale non erano state modificate regole o salvataggi. L'utente ha poi approvato il modello, implementato nell'incremento `civil-growth-4` sopra descritto; i benchmark effettivi restano da eseguire.

### 2026-10-27 — M4: diagnostica dei colli di bottiglia (validazione Windows in attesa)

- Il pianificatore espone, per ogni struttura, gli identificatori degli input insufficienti e delle risorse di uscita senza spazio, usando le stesse prenotazioni, priorità e regole del passo economico. La proiezione applicativa ne presenta i nomi leggibili; nel ciclo di avanzamento al secondo la raccolta diagnostica è disabilitata per evitare allocazioni superflue.
- Nel dettaglio Edifici compaiono le cause specifiche (per esempio pesce mancante o cibo senza capacità), i prerequisiti edilizi assenti e la richiesta di personale. Le strutture parzialmente attive indicano il numero di lavoratori produttivi.
- Aggiunti tre test per diagnostica Core, proiezione Application e dettaglio Avalonia; rafforzati test esistenti sulla contesa di input e capacità.
- Nessun nuovo pacchetto, regola di gioco, conversione o modifica al formato SQLite. **I test nuovi e i render sono ancora da eseguire su Windows**: nell'ambiente di preparazione non è disponibile `dotnet`. Restano confermati solo i 140 test del checkpoint precedente.

### 2026-10-27 — Script di pulizia

- Aggiunti `clean.cmd` e `scripts/clean.ps1`: cancellano `bin` e `obj` di ogni progetto del repository, con `-DryRun` per l'anteprima. Esclusi `.git`, `.local-cache` e collegamenti.

### 2026-10-27 — M4: analisi di raggiungibilità del catalogo

- Aggiunto `CatalogReachability.Analyze` nel Core: controllo statico che ogni struttura sia costruibile (prerequisiti, costi pagabili con risorse rinnovabili o con la dotazione iniziale, capacità di base o ampliabile) e che ogni ricetta abbia input rinnovabili ed entri nella capacità. Ignora tempo, persone e contesa.
- Simulatore: `--check-catalog [--scenario …]`, con codice di uscita 1 se uno sblocco è impossibile. La validazione lo esegue per entrambi gli scenari e salva i report.
- Test sugli scenari distribuiti (completi) e sulla catena delle conversioni di `civil-growth`. **140 test superati**; regole e salvataggi invariati.

### 2026-10-27 — M4: filiera del pane e regole `civil-growth-3`

- Aggiunti grano, acqua e farina e quattro strutture: fattoria (1 grano/s), pozzo (1 acqua/s), mulino (2 grano → 1 farina) e forno (1 farina + 1 acqua → 3 cibo), con prerequisiti mulino→fattoria e forno→mulino+pozzo. Valori provvisori: il pane affianca il pesce come fonte di cibo, più efficiente per lavoratore ma più costosa.
- Nuove regole `civil-growth-3`. Le conversioni sono dichiarate in `CivilGrowthScenario.Changes` e si concatenano: i checkpoint `civil-growth-1` e `civil-growth-2` vengono caricati con le nuove strutture a zero e l'originale viene copiato come prima.
- La tabella della vista Risorse ora scorre, con intestazione e nota fisse: le otto risorse non stavano nella finestra compatta.
- Simulatore: cronologia `bread`, inclusa nella validazione. Verificati **132 test**, nove simulazioni, otto render e ciclo nativo Windows. Aggiornati scenario, formato, report M4, roadmap e handoff.

### 2026-10-27 — M4: magazzino espandibile e regole `civil-growth-2`

- Aggiunto il Magazzino (100 legno lavorato + 60 pietra, senza personale): +500 capacità per ognuna delle cinque risorse. Valori provvisori scelti dall'utente.
- Introdotto l'effetto `StoragePerBuilding` nelle definizioni: la capacità effettiva deriva dagli edifici e viene usata da costruzione, previsione, avanzamento, ripristino e vista Risorse. Limite numerico atomico.
- Nuove regole `civil-growth-2`. I checkpoint `civil-growth-1` vengono convertiti al caricamento con zero magazzini e il desktop lo spiega; regole sconosciute restano rifiutate.
- L'adattatore SQLite copia il checkpoint in `<nome>.<regole>.backup.sqlite3` prima di sostituirlo con scenario o regole diversi, senza mai sovrascrivere copie esistenti. Formato file/snapshot invariato (v2).
- UI: descrizione, effetto e stato del magazzino. Simulatore: cronologia `storage`, inclusa nella validazione.
- Verificati **128 test**, build senza avvisi, otto simulazioni, otto render e ciclo nativo Windows. Aggiunto l'[ADR 0010](docs/adr/0010-storage-capacity-and-rules-conversion.md); aggiornati scenario, formato, report M4, roadmap e handoff.

### 2026-10-26 — M4: case, crescita e prima filiera alimentare

- Aggiunto lo scenario giocabile `civil-growth-1`, con casa piccola, pescatore e deposito che trasforma pesce in cibo, oltre ai tre produttori di base.
- Introdotti capacità abitativa, consumo civile esatto e crescita di persone intere con accumulo dei secondi validi; fame, pausa e case piene sospendono gli arrivi senza perdere il progresso.
- Condivise prenotazioni alimentari e ricette tra forecast e avanzamento. La previsione delle risorse include il consumo degli abitanti.
- Aggiunti posti abitativi, stato della crescita e fabbisogno nell'interfaccia; distinte strutture abitative e produttive e saldi negativi delle risorse.
- Creato snapshot/file SQLite v2 per la crescita parziale. Il nuovo default usa `civil-growth.sqlite3`; Foundation v1 resta disponibile con `--scenario economy-foundation` e non viene convertito o sovrascritto automaticamente.
- "Nuova partita" mantiene lo scenario corrente. Aggiunti argomenti del simulatore e cronologie `bootstrap` e `recovery`.
- Verificati **117 test**, build senza avvisi, sette simulazioni fino a 32 ore, otto render e ciclo nativo Windows per entrambi gli scenari, inclusa la ripresa a 59 secondi con un solo arrivo successivo.
- Aggiornati scenario, ADR 0009, formato dei salvataggi, roadmap e handoff. Magazzini espandibili e ulteriori filiere civili restano da implementare in M4; valori e durata del gioco completo non sono ancora bilanciati.

### 2026-10-26 — Avalonia 12 e preparazione dello scenario M4

- Aggiornata la soluzione definitiva ad Avalonia **12.1.4**, con lock file coerenti; la prova storica M1 resta separata.
- Migrati i test Desktop a xUnit 3.2.2, configurato HarfBuzz per il rendering headless e aggiornata l'esportazione PNG. Timer associati esplicitamente al dispatcher della finestra.
- Aggiornato Microsoft.Data.Sqlite a **10.0.14**, che risolve SQLitePCLRaw 2.1.13 e SQLite nativo 3.53.3, dopo aver rilevato l'advisory della precedente libreria nativa.
- Aggiunta una fixture di checkpoint M3 e verificato il ripristino esatto con il nuovo provider, senza modifiche al formato v1 o alle regole economiche.
- Build Release senza errori o avvisi; **83 test superati**, quattro render, simulazioni e ciclo nativo Windows completati. Il riepilogo di validazione registra anche le versioni dei pacchetti risolte.
- Confrontate 63 coppie pacchetto/versione con il feed delle vulnerabilità NuGet: nessuna corrispondenza dopo l'aggiornamento. L'audit CLI resta impedito dall'errore TLS dell'ambiente; dettagli nel [report](docs/validation/AVALONIA_12.md).
- Registrato l'[ADR 0008](docs/adr/0008-avalonia-12.md) e preparata una [scheda M4](docs/scenarios/CIVIL_GROWTH_DRAFT.md) con valori candidati per abitazioni, crescita e alimentazione, ordine del passo e prove richieste. Queste nuove regole non sono ancora implementate.

### 2026-10-26 — M3: prima partita desktop salvabile

#### Aggiunto

- Desktop Avalonia/MVVM e Infrastructure collegati alla soluzione definitiva, con quattro progetti di test complessivi e nessun riferimento ai prototipi.
- Viste Edifici e Risorse, costi e ricette, assegnazioni e costruzioni da 1/5/10 unità, produzione/consumo/saldo previsti al secondo e capacità visibili.
- Port applicativo di persistenza, salvataggi fuori dal dispatcher e controllo di revisione contro caricamenti tardivi.
- Formato SQLite v1 con metadati, quantità in millesimi, edifici e assegnazioni in una transazione atomica.
- Caricamento iniziale in pausa, checkpoint ogni minuto, salvataggio manuale e alla chiusura; conservazione della sessione se la scrittura fallisce.
- Protezione di file estranei e versioni future, conferma di nuova partita e uscita senza salvataggio solo su scelta esplicita dopo errore.
- Orologio monotono, pausa alla minimizzazione e gestione delle notifiche di sospensione/ripresa Windows, con fallback per interruzioni lunghe.
- Comando `run-game.cmd`, pubblicazione Windows nel validatore, documentazione del formato e ADR 0007.

#### Verificato

- Build Release senza errori o avvisi; **82 test superati** (36 Core, 18 Application, 12 Infrastructure, 16 Desktop).
- Quattro render a 1240×850 e 1040×740; simulazioni di 60 secondi, 8 e 32 ore.
- Pacchetto nativo Windows: salvataggio, nuova sessione, ricaricamento, sospensione simulata e chiusura completati con codice zero.
- Comando di avvio interattivo verificato in modalità smoke, senza usare il salvataggio personale.

Baseline locale Avalonia 11.3.18 e Microsoft.Data.Sqlite 10.0.9. L'audit online è stato ritentato e non completato per errore TLS. Popolazione dinamica, alimentazione, catalogo completo, militari, isole e bilanciamento restano nelle milestone successive.

### 2026-10-26 — Nuovi riferimenti di risorse e filiere

- Analizzate due schermate delle risorse e tre diagrammi di materiali, alimentazione, monete, esercito, colonie e strutture decorative.
- Aggiornato l'inventario a nove immagini e documentati i collegamenti visibili, le ambiguità e le conseguenze sulle milestone successive.
- Distinte quantità, capacità e variazioni senza attribuire unità temporali non mostrate; mantenuti separati abitanti, materiali, missioni e mezzi.
- Aggiornati game design e handoff. Nessuna modifica al codice, ai parametri dello scenario o alle immagini.

### 2026-10-26 — M2: motore economico e casi d'uso

#### Aggiunto

- Soluzione definitiva `SimRisorse.slnx` con Core, Application, due progetti di test e strumento Simulation, senza riferimenti a bozza o esperimento.
- Catalogo immutabile con identificatori stabili, validazione di riferimenti e cicli dei prerequisiti.
- Risorse esatte in millesimi su `long`, capacità, acquisti multipli atomici e assegnazioni intere dei lavoratori.
- Pianificatore unico per produzione e previsioni, con prenotazioni di materiali e spazio per priorità esplicita.
- Tempo di gioco esplicito, pausa, residui inferiori al secondo e snapshot immutabili validati.
- Funzionalità verticali per nuova partita, costruzione, assegnazione, rilascio, pausa, avanzamento e lettura dello stato.
- Coordinamento delle mutazioni e delle proiezioni per impedire doppie spese o assegnazioni concorrenti.
- Scenario economico di riferimento, ADR 0006 e report M2.
- Configurazione SDK, build, stile e pacchetti; lock file e comando `validate.cmd`.

#### Verificato

- Build Release con zero errori e avvisi; 45 test superati (36 Core, 9 Application e architettura).
- Simulazioni senza finestra di 60 secondi, 8 e 32 ore, con tempo e quantità entro i limiti attesi.
- Configurazioni storiche della bozza e pacchetti dell'esperimento rimasti separati.

L'audit online delle dipendenze è stato tentato, ma non completato a causa di un errore TLS dell'ambiente. La nuova UI, l'adattatore SQLite definitivo e il bilanciamento completo restano lavoro successivo.

### 2026-10-26 — M1: scenario ridotto e prova desktop

#### Aggiunto

- Scheda del primo scenario economico: tre strutture, otto abitanti, acquisti e assegnazioni manuali, ricette e pausa.
- Prova eseguibile isolata in `experiments/stack-probe/`, con modello indipendente dalla UI, finestra Avalonia/MVVM e adattatore SQLite.
- Salvataggio transazionale, caricamento validato e ripresa in pausa, compreso il tempo residuo inferiore al secondo.
- 23 test per economia, tempo, conservazione dei lavoratori, persistenza e interfaccia headless con rendering Skia.
- Script riproducibile di ripristino, build, test, pubblicazione e avvio nativo di verifica; comando per aprire la prova interattiva.
- Versioni centralizzate e lock file limitati all'esperimento.
- Report M1 con evidenze e limiti delle verifiche.

#### Verificato e deciso

- Build Release con zero errori e avvisi; tutti i 23 test superati; pacchetto framework-dependent avviato su Windows.
- Rendering a 1180×790 e 1020×720, con comandi principali visibili; cronometro oltre 24 ore.
- Adozione di C#/.NET 10, Avalonia/MVVM e SQLite per la prima implementazione desktop, registrata nell'ADR 0005.

La prova usa pacchetti già disponibili offline: non certifica versioni più recenti, sicurezza delle dipendenze, altri sistemi operativi o bilanciamento del gioco completo. Il Core definitivo resta M2.

### 2026-10-26 — Impostazione del progetto

#### Aggiunto

- README con descrizione, stato reale del progetto e percorsi di lettura.
- Game design con scelte confermate, proposte e registro delle questioni aperte.
- Architettura prevista, convenzioni di sviluppo e criteri di verifica.
- Registro ADR con cinque decisioni/proposte e modello per quelle successive.
- Roadmap per milestone e handoff per la ripresa del lavoro.
- Indice e analisi dei quattro screenshot forniti come riferimento.
- README del prototipo con limiti noti e istruzioni per aprirlo.

#### Riorganizzato

- Soluzione, sorgenti e test Windows Forms trasferiti in `legacy/winforms/`, senza modifiche al loro contenuto e mantenendo i riferimenti relativi.
- Immagini trasferite da `screnshot/` a `docs/references/screenshots/`, senza rinominarle.
- Cache Visual Studio, output `bin/` e `obj/`, preferenze locali del progetto e cartelle vuote di analisi raccolti in `.local-cache/`. La pulizia è reversibile: questi file non sono stati cancellati.

#### Configurazione

- Aggiunte esclusioni Git mirate per cache locali, dati di gioco locali e file ausiliari SQLite.

### Limiti della modifica

- Nessuna modifica alle meccaniche della bozza.
- Nessuna migrazione del runtime, nuova applicazione Avalonia o persistenza SQLite implementata.
- Nessuna promessa di completamento in otto ore verificata attraverso simulazioni.
