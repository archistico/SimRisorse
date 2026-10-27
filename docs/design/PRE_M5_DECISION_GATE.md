# SimRisorse — D0: dossier decisionale prima di M5

Data: 2026-10-27. **D0.1 approvata:** [prima campagna con 22 tipi](FIRST_CAMPAIGN_PROPOSAL.md), di cui 12 presenti e 10 futuri; gli altri 21 riferimenti restano fuori dalla prima campagna. **Altre regole D0 ancora da scegliere.** Il [gate Windows M4 è verificato](../validation/M4_MEASURED_RESULTS.md), 177/177 test, lettore dei benchmark eseguito e nove render ispezionati. Le regole eseguibili restano `civil-growth-5`. La [scheda decisionale D0](D0_DECISION_BOARD.md) distingue perimetro scelto, proposte e condizioni prima del codice M5; gli studi industriali, militari e navali non fissano prezzi o regole approvati.

## 1. Perché esiste questo gate

La visione confermata è costruire i tipi di edificio previsti dallo scenario nel minor **tempo di gioco**, facendo crescere economia, popolazione e poi componente militare/navale. La GUI finale sarà **pixel art retro 8-bit essenziale**, per **elenchi e schede**, senza collocamento sulla mappa. Le durate **8 ore** per una partita molto efficiente e **32+ ore** per una meno efficiente sono obiettivi, non risultati misurati.

I diagrammi di riferimento mostrano **43 tipi strutturali** (oltre a soldati, navi e missioni); il catalogo civile corrente implementa **12 tipi**. L'utente ha scelto un primo catalogo di **22 tipi**, senza richiedere la riproduzione di tutti i 43 riferimenti. La regola esatta con cui gli edifici contano per vincere resta D0.2. Definire i ruoli prima dei numeri previene edifici senza funzione e vicoli ciechi nelle dipendenze.

**Tre etichette diverse da non confondere:**

- **Presente:** un edificio equivalente è già implementato nello scenario corrente `civil-growth-5`; non implica che sia obbligatorio nella vittoria finale.
- **Da valutare:** compare nei diagrammi ma non è una struttura approvata o eseguibile in SimRisorse.
- **Candidato a obiettivo:** proposta di classificazione per la vittoria, sempre da approvare. Un elemento opzionale può comunque avere utilità strategica.

## 2. Inventario tracciabile dei 43 tipi di riferimento

Fonte: [Analisi delle filiere e dei tre diagrammi](../references/RESOURCE_AND_PRODUCTION_CHAINS.md). La corrispondenza dei depositi è **funzionale**, non una replica delle tre dimensioni mostrate: oggi esiste una sola versione di Magazzino e una sola di Deposito alimentare. Ogni numero della tabella identifica un tipo di riferimento, non un ID già presente nel codice.

| N. | Gruppo | Tipo raffigurato | Corrispondenza attuale | Valutazione iniziale dell'inventario, prima della scelta D0.1 |
| ---: | --- | --- | --- | --- |
| 01 | Materiali | Taglialegna | `tree-feller` | Presente; definire ruolo nella vittoria |
| 02 | Materiali | Segheria | `sawmill` | Presente; definire ruolo nella vittoria |
| 03 | Materiali | Tagliapietra | `stonecutter` | Presente; definire ruolo nella vittoria |
| 04 | Depositi e case | Deposito generale, primo taglio | `warehouse` (equivalenza funzionale) | Presente; scala a livelli non implementata |
| 05 | Depositi e case | Deposito generale, secondo taglio | — | Da valutare: variante di capacità oppure upgrade |
| 06 | Depositi e case | Deposito generale, terzo taglio | — | Da valutare: variante di capacità oppure upgrade |
| 07 | Depositi e case | Casa piccola | `small-house` | Presente; 8 posti nello scenario civile |
| 08 | Depositi e case | Casa intermedia | — | Da valutare: utilità rispetto a piccola/grande |
| 09 | Depositi e case | Casa grande | `large-house` | Presente; 32 posti, richiede Forno |
| 10 | Alimentazione | Fattoria | `farm` | Presente; Grano |
| 11 | Alimentazione | Mulino | `mill` | Presente; Farina |
| 12 | Alimentazione | Panificio | `bakery` | Presente; Pane |
| 13 | Alimentazione | Pozzo | `well` | Presente; Acqua |
| 14 | Alimentazione | Pescatore | `fisher` | Presente; Pesce |
| 15 | Alimentazione | Cacciatore | — | Da valutare: Carne e funzione distinta |
| 16 | Alimentazione | Allevamento di maiali | — | Da valutare: Carne da Grano e Acqua |
| 17 | Alimentazione | Deposito alimentare, primo taglio | `food-store` (equivalenza funzionale) | Presente; attuale ricetta Pesce → Cibo |
| 18 | Alimentazione | Deposito alimentare, secondo taglio | — | Da valutare: nuova capacità oppure upgrade |
| 19 | Alimentazione | Deposito alimentare, terzo taglio | — | Da valutare: nuova capacità oppure upgrade |
| 20 | Metalli e moneta | Miniera d'oro | — | M5.1 candidato; minerale d'oro |
| 21 | Metalli e moneta | Miniera di carbone | — | M5.1 candidato; input condiviso |
| 22 | Metalli e moneta | Miniera di ferro | — | M5.1 candidato; minerale di ferro |
| 23 | Metalli e moneta | Fusione dell'oro | — | M5.1–M5.2 candidato; oro |
| 24 | Metalli e moneta | Fusione del ferro | — | M5.1–M5.2 candidato; ferro |
| 25 | Metalli e moneta | Zecca | — | M5.2 candidato; monete |
| 26 | Esercito e navigazione | Forgia | — | M5.2 candidato; armi |
| 27 | Esercito e navigazione | Caserma | — | M5.3 candidato; addestramento |
| 28 | Esercito e navigazione | Sede iniziale | — | Da decidere: edificio costruibile o dotazione iniziale |
| 29 | Esercito e navigazione | Torre di guardia | — | M5.7 candidato; funzione da definire |
| 30 | Esercito e navigazione | Castello | — | M5.7 candidato; progressione/traguardo |
| 31 | Esercito e navigazione | Cantiere navale | — | M5.4 candidato; navi |
| 32 | Colonie | Piantagione di banane | — | M5.6 candidato; risorsa/ricompensa da definire |
| 33 | Colonie | Piantagione di caffè | — | M5.6 candidato; risorsa/ricompensa da definire |
| 34 | Colonie | Piantagione di cotone | — | M5.6 candidato; risorsa/ricompensa da definire |
| 35 | Decorazioni | Arc de Triomphe | — | M6 candidato; preferenza preliminare: opzionale |
| 36 | Decorazioni | Brandenburg Gate | — | M6 candidato; preferenza preliminare: opzionale |
| 37 | Decorazioni | Liberty Statue | — | M6 candidato; preferenza preliminare: opzionale |
| 38 | Decorazioni | Big Ben | — | M6 candidato; preferenza preliminare: opzionale |
| 39 | Decorazioni | Eiffel Tower | — | M6 candidato; preferenza preliminare: opzionale |
| 40 | Decorazioni | Fernsehturm Berlin | — | M6 candidato; preferenza preliminare: opzionale |
| 41 | Decorazioni | Ayers Rock | — | M6 candidato; preferenza preliminare: opzionale |
| 42 | Decorazioni | Niagara Falls | — | M6 candidato; preferenza preliminare: opzionale |
| 43 | Decorazioni | Grand Canyon | — | M6 candidato; preferenza preliminare: opzionale |

**Quadratura:** 3 materiali + 6 depositi/case + 10 alimentazione + 6 metalli/moneta + 6 esercito/navigazione + 3 colonie + 9 decorazioni = **43**. Le corrispondenze funzionali con i tipi implementati sono **12**; degli altri **31**, dieci sono scelti per la prima campagna e 21 sono fuori dal suo catalogo. La [classificazione D0.1 corrente](FIRST_CAMPAIGN_PROPOSAL.md) aggiorna le valutazioni iniziali dell'inventario. La scelta dei tipi futuri non ne approva automaticamente i parametri economici.

## 3. Pacchetti di decisioni e dipendenze

| Pacchetto | Decisione richiesta | Blocca | Evidenza per approvarlo |
| --- | --- | --- | --- |
| **D0.1 — Catalogo** | Per ogni tipo: obbligatorio, alternativo, facoltativo, escluso; ruolo distinto e collocazione M5/M6 | Vittoria e grafo dei prerequisiti | Lista finita con motivazione e identificatori univoci |
| **D0.2 — Vittoria / Q-01** | Possesso attuale, costruzione storica o traguardo finale; comportamento dopo demolizione | Condizione di fine partita e classifica | 5 esempi di vittoria/non vittoria privi di ambiguità |
| **D0.3 — Isole / Q-02** | Quante conquiste servono e che cosa sbloccano; quali opzionali | Missioni, navigazione, catalogo | Percorso fino al traguardo e caso di missione fallita |
| **D0.4 — Militari / Q-09** | Bacino di popolazione, costo armi, reclutamento, durata, perdite/congedo | Caserma, missioni e limite 8 ore | Bilancio dei civili/lavoratori/soldati sempre coerente |
| **D0.5 — Tempo e classifica** | Accelerazione, pause, salvataggi ripresi, punteggio e difficoltà | Simulazione di vittoria e risultati M6 | Tempo di gioco definito in ogni situazione |
| **D0.6 — Raggiungibilità** | Grafo di dipendenze completo con risorse e capacità di accumulo | Avvio M5.1 | Almeno un percorso teoricamente percorribile; nessun prerequisito ciclico |
| **D0.7 — Presentazione 8-bit** | Schermata campione e organizzazione delle future sezioni | Prototipo grafico, non il Core | Cibo/Pane, edifici e blocchi leggibili a 1040×740 e 1240×850 |
| **D0.8 — Armi e monete** | Spade consumabili o dotazione persistente; Monete come risorsa mirata oppure valuta generale; eventuali fonti coloniali | M5.2–M5.4, contratto di spesa e salvataggi | Due esempi tracciabili: reclutamento e costruzione nave, con costo e rollback espliciti |

La scelta di un ruolo per un edificio viene **prima** di costi e rendimenti; questi saranno proposti e misurati nei singoli incrementi. Una scheda minima futura per tipo deve contenere `id`, categoria, ruolo, obbligatorietà, prerequisiti, costo, numero lavoratori, input/output per secondo, capienza/benefici, sblocco, UI e test di raggiungibilità. Non riempire i campi economici con valori arbitrari.

## 4. Alternative su cui chiedere una decisione

### D0.2 — Come si vince?

| Opzione | Funzionamento | Pro | Rischio |
| --- | --- | --- | --- |
| A. Possesso simultaneo | Tutti i tipi obbligatori presenti nello stesso istante | Obiettivo concreto e controllabile | Favorisce il mantenimento di strutture obsolete; demolizione delicata |
| B. Costruzione storica | Ogni tipo obbligatorio costruito almeno una volta | Libertà di riconvertire l'economia | Si può vincere avendo già smantellato l'intera produzione |
| **C. Obiettivi + traguardo** | Insieme finito di costruzioni/missioni richieste, più obiettivo conclusivo esplicito | Dà senso alla catena economica e alle isole | Richiede definire con precisione la prova finale |

**Preferenza tecnica provvisoria: C**, con decisione distinta su quali edifici vadano ancora posseduti al momento della vittoria. Non è approvata.

### D0.3 — Quanto contano le isole?

| Opzione | Effetto sul gioco | Rischio |
| --- | --- | --- |
| A. Tutte facoltative | Ricompense migliorano la strategia ma non la vittoria | Navigazione marginale |
| **B. Un percorso essenziale, resto facoltativo** | Almeno una spedizione e conquista necessarie a uno sblocco finale | Calibrare costi, tempi e recupero da fallimento |
| C. Tutte necessarie | Vittoria legata a ogni conquista | Durata e ripetitività difficili da controllare |

**Preferenza tecnica provvisoria: B**; è coerente con la visione militare, senza rendere ogni isola un ostacolo obbligatorio.

### D0.4 — Come si contano le persone?

Il modello da valutare è: `popolazione totale = civili impiegati + civili disponibili + reclute + soldati`, senza contare una persona due volte. L'eventuale rientro di un soldato tra i civili, le perdite e la necessità di Pane/Cibo per le truppe restano **decisioni aperte**. Il consumo civile e la crescita di M4 rimangono invariati finché non viene approvata una nuova versione delle regole.

## 5. Vincoli misurabili per non costruire un gioco irraggiungibile

Con regole civili **attuali `civil-growth-5`**, 8 abitanti iniziali e l'ipotesi deliberatamente irrealistica di avere fin dal tempo zero case sufficienti, bonus a **2×**, Cibo e Pane sempre disponibili, il tetto aritmetico è **968 abitanti in 8 ore** e **3.848 in 32 ore**: sono limiti ideali, non popolazioni garantite. Il confronto **storico `civil-growth-4`** a ritmo fisso di un arrivo ogni 60 secondi ha invece tetti **488/1.928**. Nella partita reale il costo delle case, i prerequisiti, la manodopera e le carenze riducono il risultato. L'[analisi M4.13](../scenarios/HOUSING_INCENTIVE_AUDIT.md) mostra perché anche a parità di 2× le combinazioni abitative possono avere capacità e costi alimentari diversi. L'esercito non può richiedere più residenti di quelli realmente disponibili nella cronologia.

Un percorso candidato alla vittoria dovrà inoltre superare questi controlli:

1. **Materie iniziali e produzioni rinnovabili:** ogni costo futuro può essere finanziato senza una risorsa ottenibile soltanto costruendo il medesimo edificio.
2. **Magazzino:** la capacità permette di accumulare il costo massimo di un ordine obbligatorio; nessuna necessità implicita di superare il proprio limite.
3. **Lavoratori e alloggi:** domanda simultanea di personale e reclute compatibile con la popolazione raggiungibile.
4. **Tempo:** almeno una strategia di riferimento completa la progressione entro una durata da misurare in M6, non assunta oggi.
5. **Isole e navi:** non si richiede una risorsa coloniale per costruire l'unico mezzo capace di raggiungere la colonia che la fornisce.
6. **Partite salvate:** costruzioni, reclute e missioni conservano lo stato e il tempo simulato senza duplicazioni di ricompense.

## 6. Contenuti da non introdurre per anticipazione

- **Carne:** ancora aperto se sia Cibo alternativo o vettovagliamento militare. Preferenza documentata: discuterla con le missioni M5, non aggiungerla all'attuale `civil-growth-5` ora. [Studio](../scenarios/MEAT_DESIGN_REVIEW.md).
- **Demolizione:** rimborso del 25% soltanto candidato; senza una politica sulle capacità residue non è una funzione pronta. [Studio](../scenarios/DEMOLITION_M4_REVIEW.md).
- **Decorazioni:** nove riferimenti non equivalgono automaticamente a nove obblighi di vittoria. Devono aggiungere scelta, valore collezionistico o un traguardo, altrimenti restare fuori dalla prima versione.
- **GUI finale:** sprite e icone originali; prima un prototipo con Pane/Cibo/Case/Forno, poi industria, soldati e isole. Non introdurre una mappa di costruzione. [Contratto grafico](../PIXEL_ART_UI.md).

## 7. Come proseguire dopo la verifica Windows e la scelta del catalogo

1. **Evidenza acquisita:** gate Windows da 177/177 test, nove render ispezionati e benchmark letti; conservare le misure del rapporto M4 come baseline.
2. **Catalogo scelto:** registrata D0.1 sui 22 tipi; Carne/Cacciatore/Allevamento fuori dalla prima campagna. Restano giudizio utente sul prototipo e classificazione della demolizione per la chiusura M4.
3. **In revisione con l'utente:** scegliere D0.2/D0.3 usando la [proposta concreta e i casi di vittoria](FIRST_CAMPAIGN_PROPOSAL.md), poi D0.4/D0.5/D0.8; aggiornare il game design solo per le risposte effettive.
4. **Prima di M5.1:** completare prerequisiti, percorso quantitativamente raggiungibile e scelta S1/S2. Dimensionare la prima filiera senza anticipare ricette o migrazioni non scelte.

**Criterio d'uscita D0:** non basta riempire la lista dei 43 riferimenti. Serve un catalogo finito scelto, con ruoli distinguibili, una condizione di vittoria non ambigua, almeno una missione/isola progettata e un percorso verificabile che non richieda risorse, personale o tempo impossibili.

**Prima analisi eseguibile dopo il gate:** lo [studio M5.1 su ferro, carbone e oro](M5_1_INDUSTRY_PREFEASIBILITY.md) propone incrementi piccoli, due set di prezzi alternativi, capacità e rischi di migrazione. È un dossier tecnico, non un'autorizzazione a introdurre le ricette nel catalogo.

Lo [studio M5.2 su Forgia e Zecca](M5_2_FORGE_MINT_PREFEASIBILITY.md) esamina l'uso successivo di Ferro e Oro, con una dimostrazione del collo di bottiglia del Carbone e una scelta aperta fondamentale: **Monete come scorta consumabile per alcuni comandi o valuta generale**. Sono soltanto ipotesi; D0.8 deve essere chiuso prima dell'implementazione.

Lo [studio M5.3 su Caserma e addestramento](M5_3_ARMY_PREFEASIBILITY.md) confronta tre modelli di reclutamento e formalizza la conservazione della popolazione fra lavoratori, civili liberi, reclute e soldati. Non fissa costi, durate, razioni né perdite; D0.4 e D0.8 sono prerequisiti alla sua implementazione.

Lo [studio M5.4 su Cantiere navale e flotta](M5_4_NAVAL_PREFEASIBILITY.md) distingue i costi degli scafi da quelli del Cantiere e le navi riutilizzabili da quelle monouso. Non fissa capacità, equipaggio, prezzi, missioni o ritorni; D0.3, D0.4, D0.8, raggiungibilità e S1/S2 devono essere risolti prima del codice.
