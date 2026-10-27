# Game design

Data di riferimento: 2026-10-27. Questo documento distingue requisiti confermati, proposte e questioni da risolvere; gli screenshot sono evidenze di un riferimento, non una specifica da copiare integralmente. L'ordine operativo delle scelte è nella [roadmap](ROADMAP.md).

## Visione e ciclo di gioco

Il giocatore costruisce un'economia partendo da materiali di base. Investimenti diversi in edifici e lavoratori cambiano la velocità di progressione. L'obiettivo è arrivare a costruire tutti i tipi di edificio previsti dallo scenario nel minor tempo di gioco possibile.

Il ciclo previsto è: osservare risorse e fabbisogni, scegliere un investimento, organizzare il lavoro, far avanzare la simulazione, raggiungere nuovi sblocchi e correggere i colli di bottiglia. La visione comprende soldati, castelli e missioni di conquista delle isole.

## Requisiti confermati

**Decisione D0.1 del 2026-10-27:** prima campagna con i [22 tipi elencati nella scheda dedicata](design/FIRST_CAMPAIGN_PROPOSAL.md): 12 civili, cinque di estrazione/fusione, Forgia, Zecca, Caserma, Cantiere e Castello. Gli altri 21 riferimenti sono fuori dalla prima campagna. Con M5.1b **17 tipi sono implementati e 5 restano futuri**.

**Decisioni successive esplicite:** vittoria con possesso di tutti i 22 tipi e Castello conclusivo; una conquista obbligatoria sblocca il Castello. Gli edifici iniziali contano già. M4 chiusa dopo giudizio positivo sul prototipo monospace e rinvio della demolizione. Scelta **S1** per continuare le partite con conversione/backup e **configurazione A per la sola prima filiera ferro/carbone**. M5.1a verificata nelle regole **`civil-growth-6`**, 15 edifici e 12 risorse: [parametri e risultati](validation/M5_1A_IRON.md). Le regole operative militari/navali, l'oro e la funzione di vittoria restano da implementare nei rispettivi incrementi.

**Scelta successiva M5.1b:** approvata separatamente la configurazione A dell'Oro: Miniera 180/120 legno/pietra, 1 minerale/s; Fonderia 220/170, 1 minerale + 1 carbone → 1 oro/s; un addetto ciascuna, capacità 1.000 +500 per Magazzino, scorte iniziali zero. Ferro prioritario con carbone scarso. **M5.1b verificata**, `civil-growth-7`, 17 edifici / 14 risorse e 204/204 test: [report](validation/M5_1B_GOLD.md). Forgia, Zecca e relativi usi rimangono futuri.

| ID | Requisito | Conseguenza |
| --- | --- | --- |
| G-01 | Costruire tutti i tipi di edificio è il traguardo generale | Serve un insieme di obiettivi finito per ciascuno scenario |
| G-02 | Edifici successivi più difficili da ottenere | La progressione introduce costi e dipendenze crescenti |
| G-03 | Il numero di edifici influenza il tempo di completamento | Il giocatore sceglie quali capacità produttive espandere |
| G-04 | Gestione tramite elenco | Nessuna collocazione spaziale degli edifici nel perimetro iniziale |
| G-05 | Lavoratori presenti nel gioco | Costruzioni e disponibilità di personale sono vincoli distinti |
| G-06 | Soldati, castelli e missioni di conquista delle isole | La progressione militare appartiene alla visione del prodotto |
| G-07 | Il gioco si ferma durante la pausa e quando non si gioca | Nessuna produzione a gioco chiuso o recupero del tempo trascorso |
| G-08 | Classifica basata sul tempo di gioco | Serve una misura del tempo separata dall'orologio del calendario |
| G-09 | Partita molto efficiente di circa 8 ore; strategie meno efficaci anche oltre 32 ore | Obiettivo da misurare, non durata già garantita |
| G-10 | GUI definitiva in pixel art retro 8-bit essenziale | Stile grafico confermato, da introdurre senza sacrificare la leggibilità |

Le otto ore indicano tempo effettivamente avanzato nella simulazione. Il tempo speso a riflettere in pausa non viene conteggiato. Il rapporto iniziale tra tempo reale e simulato resta in Q-04.

Per la sola prova M1 sono state definite [regole operative provvisorie](scenarios/ECONOMY_PROBE.md): tre strutture, otto abitanti, acquisti immediati, assegnazioni manuali, passi di un secondo e ordini in pausa. Q-03, Q-04, Q-06 e Q-07 hanno quindi una risposta limitata all'esperimento; non sono chiuse per il gioco completo.

M2 ha trasferito il piccolo ciclo nello [scenario economico del Core](scenarios/ECONOMY_FOUNDATION.md), con acquisti e assegnazioni multipli, risorse frazionarie e priorità dichiarate. I valori restano di riferimento, non bilanciamento finale. Q-07 ha una politica implementata nel primo motore: prenotazioni per priorità, ricette complete e materiali disponibili al passo successivo; una diversa politica futura richiederà un cambio esplicito delle regole.

Le cinque nuove immagini di risorse e strutture sono analizzate nei [riferimenti delle filiere](references/RESOURCE_AND_PRODUCTION_CHAINS.md). Esplicitano alimentazione, monete, armi, colonie e tre catene decorative: sono riferimenti per progettare i contenuti futuri, senza introdurre automaticamente nuove regole nel Core o chiudere le domande sull'obiettivo finale.

M3 rende giocabile lo scenario ridotto: ordini da 1, 5 o 10 unità, viste Edifici/Risorse, pausa e ripresa da checkpoint. La versione corrente usa tempo reale 1:1 durante l'esecuzione, ordini consentiti in pausa, pausa alla minimizzazione/sospensione e ripresa manuale; perdere il focus da solo non interrompe il gioco. I flussi visibili sono previsioni per il prossimo secondo di gioco e non produzione offline. Queste politiche sono formalizzate nell'[ADR 0007](adr/0007-desktop-persistence-and-lifecycle.md).

## Proposte di lavoro non ancora confermate

### Economia e costruzioni

- Catalogo di tipi di edificio con costi, requisiti, posti di lavoro, ricette ed effetti sulla capacità.
- Edifici organizzati per categorie: produzione di base, trasformazione, abitazioni, depositi, alimentazione, esercito e navigazione.
- Investimenti che richiedono tempo per ripagarsi; espandere la produzione vicino alla vittoria può non convenire.
- Quantità aggregate per tipo, con suddivisioni interne solo quando servono per lavoratori, avanzamento delle costruzioni o stato operativo.
- Possibilità di demolire e recuperare parte dei materiali, con regole ancora da definire.

### Popolazione

- Un bacino condiviso tra abitanti disponibili, lavoratori, reclute e soldati.
- Reclutamento che sottrae persone all'economia civile e addestramento che richiede tempo e risorse.
- Abitazioni con capacità e possibile crescita della popolazione; eventuale dotazione immediata di abitanti trattata come effetto separato.
- Cibo come vincolo di mantenimento. Per lo scenario civile M4 è adottata una prima politica esplicita; per soldati e gioco completo soglie e conseguenze restano aperte.

Le persone sono conteggi interi. Un tasso di crescita frazionario può accumulare avanzamento senza rappresentare una frazione di lavoratore assegnabile. Non è deciso come gestire eventuali rientri, congedi o perdite.

### Esercito e isole

- Missioni gestite da schede, senza controllo tattico diretto.
- Isole che possono offrire risorse, capacità produttiva o sblocchi.
- Durata delle missioni legata al medesimo tempo di simulazione dell'economia.
- Esiti inizialmente deterministici e informazioni sufficienti a pianificare una spedizione.
- Navi e contingenti come entità distinte dagli edifici, anche quando l'interfaccia usa elenchi simili.

L'utente ha scelto **una conquista obbligatoria che sblocca il Castello**, costruzione conclusiva. Identità dell'isola, forze, durata, costi ed esiti restano da definire prima del codice militare/navale.

### Interfaccia

- **Direzione e prototipo approvati il 2026-10-27:** pixel art retro 8-bit essenziale con font monospace, elenchi/schede e informazioni leggibili. Il [contratto grafico](PIXEL_ART_UI.md) descrive la base accettata; rifiniture, DPI fisici e nuove sezioni richiedono ulteriori verifiche.
- Riepilogo persistente di risorse, tempo e stato della partita.
- Elenco filtrabile con quantità, personale, produttività e motivi di blocco.
- Scheda di dettaglio con costo, consumo, produzione, requisiti ed effetti dell'acquisto.
- Operazioni multiple per ridurre il numero di clic quando si gestiscono centinaia di edifici.
- Informazioni testuali per gli stati: un colore, da solo, non deve essere l'unica spiegazione.

## Questioni aperte

| ID | Decisione da prendere | Prima di |
| --- | --- | --- |
| Q-01 | **Scelta:** possedere tutti i 22 tipi, Castello conclusivo; funzione da implementare | Vittoria M6 |
| Q-02 | **Ruolo scelto:** una conquista obbligatoria sblocca il Castello; identità e parametri della missione da fissare | Missioni e bilanciamento |
| Q-03 | Costruzione immediata o con durata? Code, parallelismo e acquisti multipli? | Prima funzionalità di costruzione |
| Q-04 | Velocità iniziale 1:1? Ordini applicabili in pausa? Come trattare minimizzazione e perdita del focus? | Collegamento tra interfaccia e orologio |
| Q-05 | Dotazione iniziale, abitazioni, crescita, consumo alimentare e carenze? | Popolazione dinamica |
| Q-06 | Assegnazione manuale o automatica dei lavoratori? Produttività con personale incompleto? | Economia minima |
| Q-07 | Disponibilità di materiali condivisi: priorità, ripartizione e produzione parziale? Destinazione dell'eccesso a magazzino pieno? | Algoritmo di produzione |
| Q-08 | **Rinviata dall'utente**; rimborso e capacità residue da scegliere prima di una futura implementazione | Futuro incremento demolizione |
| Q-09 | Truppe, addestramento, costi, perdite, durata, esiti e trasporti delle missioni? | Modulo militare |
| Q-10 | Classifica solo locale o pubblica? Difficoltà, pareggi, uso di salvataggi precedenti e partite valide? | Risultati competitivi |

Non tutte le domande bloccano il primo prototipo. Per l'economia minima bastano Q-03, Q-06 e Q-07, una dotazione iniziale e una regola esplicita di avanzamento del tempo. Le altre vanno risolte prima della funzionalità interessata.

Per Q-05 è implementata una prima risposta nello [scenario Civil Growth](scenarios/CIVIL_GROWTH.md): Casa piccola con 8 posti, Casa grande con 32 posti e prerequisito Forno, un arrivo ogni **60 secondi validi al ritmo base**, accelerabile dalle case aggiuntive fino a **uno ogni 30 secondi** con `civil-growth-5` quando ci sono spazio e risorse. Tutti consumano 0,010 Cibo/abitante/s; i residenti oltre la capacità complessiva delle Case piccole consumano anche 0,015 Pane/abitante/s. La filiera pescatore→deposito alimentare produce Cibo; fattoria, pozzo, mulino e forno producono Pane distinto, necessario a far crescere la popolazione avanzata. Le carenze fermano la crescita senza decessi, senza cancellare il progresso già accumulato e senza consumi a gioco chiuso. Sono valori provvisori approvati per M4, non una decisione sul futuro mantenimento dei soldati. L'[audit M4.13](scenarios/HOUSING_INCENTIVE_AUDIT.md) verifica l'effetto del limite 2× e il costo dei posti senza sostituire i benchmark. La [proposta originaria](scenarios/CIVIL_GROWTH_DRAFT.md) rimane un riferimento storico; Economy Foundation conserva le proprie regole.

Per Q-08 è disponibile uno [studio di demolizione](scenarios/DEMOLITION_M4_REVIEW.md) con esempi di rimborsi e controlli su abitanti, lavoratori e magazzini. Il rimborso del 25% è una **proposta, non una regola approvata**; la demolizione resta assente.

## Gate di design prima della milestone M5

**Stato: M4 chiusa; D0.1, D0.2, ruolo D0.3, base D0.7, S1 e configurazione A M5.1a scelti. M5.1a verificata.** La [scheda prima campagna](design/FIRST_CAMPAIGN_PROPOSAL.md) raccoglie catalogo e vittoria; la [scheda operativa](design/D0_DECISION_BOARD.md) distingue le scelte ricevute dai parametri militari/navali ancora aperti. Le preferenze non confermate rimangono proposte. Ogni fase successiva deve dimostrare il proprio percorso raggiungibile; il primo ferro è verificato da nuova partita a 1.102 s.

| Decisione | Cosa fissare | Perché condiziona M5/M6 |
| --- | --- | --- |
| **Catalogo finito — D0.1 scelta** | 22 tipi nella prima campagna, altri 21 riferimenti fuori; regole dei dieci tipi nuovi da dimensionare | Consente un grafo di dipendenze e un criterio di vittoria misurabile |
| **Q-01: vittoria** | Possesso simultaneo, costruzione storica o traguardo finale; effetto di eventuali demolizioni | Determina cosa si deve testare e quando fermare il tempo |
| **Q-02: isole** | Conquiste obbligatorie/facoltative, risorse esclusive, ricompense e requisiti | Impedisce che la navigazione sia decorativa oppure blocchi involontariamente la vittoria |
| **Q-09: militari** | Bacino reclute/lavoratori, durata addestramento, armi, cibo, costi, perdite e congedi | Evita che l'esercito superi la popolazione effettivamente disponibile |
| **Tempo e difficoltà** | Regole del gioco completo a 1:1 o accelerate; effetto sulla classifica | Rende confrontabili i tempi e le strategie di riferimento |
| **Schede dei contenuti** | Per tipo: costo, prerequisiti, lavoratori, input/output, capacità e funzione | Permette cataloghi raggiungibili e simulazioni ripetibili |

I [diagrammi di riferimento](references/RESOURCE_AND_PRODUCTION_CHAINS.md) contengono **43 tipi strutturali distinti**, incluse decorazioni; il catalogo attuale `civil-growth-5` ne implementa **12**. I 43 sono **riferimenti da vagliare**, non l'obbligo automatico di costruirli tutti. Navi, soldati, isole e missioni richiedono contratti propri.

**Vincolo per il bilanciamento:** con 8 persone iniziali e un arrivo ogni 60 secondi validi, le regole **storiche** `civil-growth-4` avrebbero un tetto di **488 persone** dopo otto ore. Con `civil-growth-5` e il bonus già a **2× dal tempo zero**, il nuovo limite *puramente aritmetico* è **968 persone** dopo otto ore; nessuna strategia iniziale può disporre gratuitamente di tutta la capienza, del Pane e delle filiere necessarie. Sono limiti ideali, **non** risultati giocati. Per condividere popolazione civile e militare occorre dimostrare che almeno una sequenza di vittoria efficiente resti raggiungibile. La durata obiettivo di circa 8 ore non è ancora stata validata per il gioco completo.

Lo [studio quantitativo M5.1](design/M5_1_INDUSTRY_PREFEASIBILITY.md) conserva il confronto A/B. I parametri A di Ferro e Oro sono scelti e integrati con approvazioni distinte; **prezzi B e filiere a valle restano ipotesi**.

Il successivo [studio M5.2 su Forgia, Zecca, Spade e Monete](design/M5_2_FORGE_MINT_PREFEASIBILITY.md) valuta l'utilizzo dell'industria prima di introdurre Caserma e Cantiere navale. Una miniera di Carbone ipotetica da 2/s non riuscirebbe a sostenere simultaneamente quattro trasformatori che richiedono in totale 4/s, ma una **produzione organizzata per fasi** potrebbe ridurre il numero di miniere necessarie. È un'alternativa strategica da misurare, non una nuova regola. Rimane aperto se le Monete siano una risorsa spendibile in azioni specifiche o una valuta generale, e se le Spade vengano consumate dal reclutamento o restino in dotazione.

Il [dossier M5.3 su Caserma, reclute e soldati](design/M5_3_ARMY_PREFEASIBILITY.md) distingue quattro stati della popolazione e propone il controllo di bilancio `totale = impiegati + liberi + reclute + soldati`. Confronta reclutamento immediato, coda di addestramento e posti simultanei di Caserma. **Si tratta di proposte, non di nuove meccaniche:** razioni, tempi, perdite, congedo, capienza e destino delle Spade richiedono una scelta esplicita nel gate D0.4/D0.8. La UI finale resta pixel art 8-bit per schede, senza mappa di posizionamento.

Il [dossier M5.4 su Cantiere navale e flotta](design/M5_4_NAVAL_PREFEASIBILITY.md) separa il costo della struttura da quello della nave, confronta tre modelli di mezzo e documenta identità persistenti, acquisti atomici e trasporto con capacità ancora da scegliere. La Zecca potrebbe fornire le Monete per la **prima nave**, evitando il circolo vizioso di finanziarla soltanto con una colonia non ancora raggiunta; anche questo percorso va approvato e misurato. **Nessuna nave, capacità, durata o rotta è implementata.** Le missioni appartengono a M5.5, le conquiste a M5.6.

Il gate è chiuso soltanto quando **l'utente avrà approvato** la classificazione degli edifici, la condizione di vittoria, i requisiti delle isole e il modello militare di base, con una prima verifica di raggiungibilità. La progettazione della GUI 8-bit può progredire in parallelo, senza trasformare l'estetica in regole di gioco.

## Obiettivi di bilanciamento

- Verificare prima che le filiere siano raggiungibili e che lo scenario possa essere completato.
- Confrontare una strategia di riferimento efficiente con strategie meno efficaci ma plausibili.
- Misurare tempo di completamento, attese, scorte inutilizzate, inattività degli edifici e vincoli di personale.
- Raggiungere la durata desiderata attraverso decisioni e progressione, evitando attese obbligatorie prive di alternative.
- Separare i risultati per scenario e versione delle regole; il formato esatto della classifica resta Q-10.

Le simulazioni automatiche servono per misurare e cercare errori. Non dimostrano da sole che una strategia sia matematicamente ottimale; le prove con giocatori restano necessarie per valutare leggibilità e ritmo.

## Perimetro iniziale e rinvii

La prima versione dimostrativa riguarda un piccolo ciclo economico con lavoratori e pausa. Esercito e isole rimangono nella roadmap del prodotto, ma non sono requisiti della prima dimostrazione tecnica.

Mappa con collocamento spaziale, combattimento tattico, produzione offline e gioco simultaneo tra utenti non rientrano nel perimetro iniziale. La decisione su una classifica pubblica è distinta dal multiplayer.

## Glossario

| Termine | Significato |
| --- | --- |
| Partita | Stato di una singola esecuzione dello scenario, riprendibile da salvataggio |
| Scenario | Contenuti, condizioni iniziali e obiettivi di una partita |
| Tipo di edificio | Definizione nel catalogo, distinta dalla quantità posseduta |
| Ricetta | Input, output e requisiti di una lavorazione |
| Capacità | Limite disponibile, distinto dalla quantità attualmente presente |
| Lavoratore | Persona assegnata a un'attività civile |
| Tempo di gioco | Tempo avanzato dal motore; esclude pause e chiusura |
| Versione delle regole | Identità del catalogo e delle formule usate dalla partita |

## Riferimenti

- [Analisi degli screenshot](references/README.md).
- [Decisioni architetturali](adr/README.md).
- [Roadmap](ROADMAP.md).
