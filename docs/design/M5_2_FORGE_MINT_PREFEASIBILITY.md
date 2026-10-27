# M5.2 — Forgia, Zecca e utilizzo delle risorse industriali

Data: 2026-10-27, allineamento M4.14/v28. **Stato: studio di prefattibilità, non approvato e non implementato.** Le regole eseguibili correnti sono **`civil-growth-5`**, introdotte in v24/M4.10 e riferite superate dall'utente (174 xUnit attesi). Il prototipo 8-bit v25 e gli incrementi v26–v28 attendono il gate Windows (177 xUnit attesi). Il successivo impiego delle Spade è esaminato nello [studio M5.3](M5_3_ARMY_PREFEASIBILITY.md). Prima di sviluppare M5.2 servono le [decisioni D0](D0_DECISION_BOARD.md), la chiusura M4, M5.1 validata e la scelta S1/S2 dei salvataggi. Nessuna cifra di questo dossier modifica `civil-growth-5`.

## 1. Che cosa sappiamo davvero dai riferimenti

Il [diagramma originale](../references/RESOURCE_AND_PRODUCTION_CHAINS.md) collega **Ferro + Carbone → Forgia → Spade** e **Oro + Carbone → Zecca → Monete d'oro**. Mostra inoltre Spade e abitanti verso la Caserma, e Monete con legno lavorato verso il Cantiere navale. Le monete compaiono anche come possibile risultato delle filiere commerciali coloniali. **Non indica quantità, prezzi, dipendenti, tempi, consumi permanenti né il significato delle monete come valuta.**

Tre questioni vanno tenute separate:

1. **Produzione:** Forgia e Zecca trasformano materiali già prodotti in M5.1.
2. **Impiego:** le Spade potrebbero essere consumate dal reclutamento e le Monete dal Cantiere navale; le quantità d'impiego appartengono a M5.3/M5.4 e non sono ancora stabilite.
3. **Economia:** l'eventuale uso delle Monete per acquistare *ogni* edificio sarebbe una modifica generale alla spesa di risorse civili; il riferimento **non** la dimostra. Non introdurla implicitamente.

Proposta di divisione tecnica: **M5.2a Forgia**, poi **M5.2b Zecca**, infine **M5.2c contesa sul carbone, depositi e uso a valle**. Questa sequenza permette di diagnosticare un nuovo consumatore alla volta.

## 2. Ipotesi A: costi e capacità da mettere alla prova

Le due ricette seguenti sono **ipotesi A specifiche per il benchmark**, non ricette adottate:

| Nuovo edificio | Legno lavorato / Pietra | Addetti | Input per addetto/s | Output per addetto/s | Prerequisiti candidati |
| --- | ---: | ---: | --- | --- | --- |
| **Forgia** | **240 / 180** | 1 | 1 Ferro + 1 Carbone | 2 Spade | Fonderia ferro |
| **Zecca** | **210 / 160** | 1 | 1 Oro + 1 Carbone | 3 Monete | Fonderia oro |

Risorse candidate aggiuntive: **Spade** e **Monete**. In un eventuale catalogo approvato M5.1 completo, i conteggi passerebbero da **17 edifici / 14 risorse** a **19 / 16**. Sono proiezioni, non contenuti già presenti. Un riferimento alla Fonderia come prerequisito non significa che questa debba restare sempre attiva: andrà definito se serva il **possesso attuale** o lo **sblocco storico**, secondo la futura politica dei prerequisiti.

L'opzione **B di confronto** conserva lavoratori e ricette A ma applica **1,5× ai soli prezzi delle due nuove strutture**: Forgia **360 / 270**, Zecca **315 / 240**. Il benchmark deve confrontare il tempo necessario a finanziarle *dopo* M5.1, non solo il costo assoluto. Entrambe le opzioni sono reversibili finché restano numeri su carta.

### Quadratura degli investimenti e della domanda di carbone

Le somme seguenti usano **M5.1 ipotesi A**, quindi non sono prezzi effettivi di SimRisorse. Suppongono **una sola miniera di ciascun metallo** e **una sola Forgia e Zecca**:

| Configurazione candidata | Legno / Pietra cumulativi | Lavoratori, tutte attive | Carbone prodotto/s | Carbone richiesto/s | Prodotti nominali a regime |
| --- | ---: | ---: | ---: | ---: | --- |
| M5.1 ferro+oro, una miniera carbone | **850 / 610** | **5** | **2** | **2** (fonderie) | 1 Ferro/s + 1 Oro/s |
| Aggiungere Forgia+Zecca, una sola miniera carbone | **1.300 / 950** | **7** | **2** | **4** (due fonderie + due trasformatori) | **Impossibile** tenere attive tutte e quattro le lavorazioni senza usare scorte preesistenti |
| Come sopra, aggiungere una seconda miniera carbone | **1.430 / 1.040** | **8** | **4** | **4** | 1 Ferro/s + 1 Oro/s + 2 Spade/s + 3 Monete/s, solo come limite di regime |

Verifica dei costi: `850 + 240 + 210 = 1.300` Legno; `610 + 180 + 160 = 950` Pietra; aggiungendo una miniera carbone M5.1 A da `130 / 90` si ottiene `1.430 / 1.040`. L'aggiunta di una sola Forgia richiede **+240 / 180 e un addetto**, mentre la Zecca richiede **+210 / 160 e un addetto**, sempre nell'ipotesi A.

**Conseguenza misurabile:** con una sola miniera da 2 Carbone/s, le quattro lavorazioni a regime ne vorrebbero 4/s: il disavanzo è **2/s**. Su 1.800 secondi l'offerta teorica è al massimo **3.600 Carbone**, contro **7.200** di domanda teorica a piena attività simultanea. Senza Carbone iniziale, **non esiste** una strategia che mantenga indefinitamente attive tutte e quattro a capacità nominale con quella sola miniera. Questa dimostrazione non dice quale struttura si fermerà per prima né per quanti secondi: la priorità del pianificatore e le scorte cambiano il risultato concreto.

### Una scelta strategica autentica: produrre a fasi

Con una sola miniera, una strategia candidata è **accumulare Ferro e Oro** usando le due fonderie e successivamente **spostare i lavoratori** verso Forgia e Zecca, fermando le fonderie. In astratto:

- Fase 1: con 2 Carbone/s, produrre fino a **120 Ferro e 120 Oro** in circa **120 secondi nominali a regime**.
- Fase 2: con le fonderie ferme e gli stessi 2 Carbone/s, consumare quei metalli per ottenere fino a **240 Spade e 360 Monete** in altri **120 secondi nominali a regime**.
- I **480 Carbone** richiesti dalle due fasi impongono **almeno 240 secondi** di estrazione a 2 Carbone/s, oltre all'avviamento, alle prenotazioni sul passo precedente e alle eventuali soste per personale, materiale, capacità o priorità. È un limite teorico sul carbone del piano esemplificativo, **non** la durata misurata della strategia né una dimostrazione dell'ottimo globale.
- Due miniere di carbone, se tutto il resto sostiene la produzione, consentirebbero in teoria di sovrapporre le due lavorazioni anziché alternarle, pagando **130/90 e un posto di lavoro aggiuntivo**. La seconda miniera ha quindi un beneficio *potenziale di tempo* che i benchmark dovranno quantificare.

La possibilità di riassegnare lavoratori è già presente in M4; ciò **non approva** pause automatiche dei singoli edifici, nuove politiche di priorità o bonus nascosti. Se lo smistamento per fasi risulta poco leggibile, bisognerà valutarne prima la comunicazione nella UI.

## 3. Scelte economiche che possono cambiare tutto

| Decisione da approvare | Opzione semplice candidata | Alternativa | Perché è importante |
| --- | --- | --- | --- |
| **M5.2-D1: significato Monete** | Risorsa stoccata e consumata da comandi espliciti (es. nave) | Valuta generale dell'economia | La seconda modifica il contratto di **tutti** gli acquisti e la migrazione dei salvataggi |
| **M5.2-D2: destino Spade** | Risorsa consumata quando si recluta | Equipaggiamento durevole assegnato al soldato | Cambia il controllo di duplicazioni, congedi, perdite e salvataggi |
| **M5.2-D3: commercio coloniale** | Futuro produttore alternativo di Monete | Entrata periodica separata dalle scorte | Il Core oggi proietta la capacità della **singola ricetta**; più produttori vanno trattati senza stime ingannevoli |
| **M5.2-D4: carbone** | Nessuna precedenza speciale: priorità deterministica del catalogo e diagnostica | Priorità configurabile dal giocatore | Comportamento in contesa, salvataggi e interfaccia diversi |
| **M5.2-D5: stoccaggio** | Capacità base 1.000 per Spade/Monete, +500 per Magazzino | Capacità o saldo monetario separati | Una Zecca da 3 Monete/s riempie una capienza di 1.000 in poco più di 333 s senza uscite, dopo avviamento |

**Non scegliere automaticamente** la colonna "semplice". Serve conferma esplicita, e le regole M4 su Cibo/Pane, Case e costo delle costruzioni rimangono valide finché non viene approvata una nuova versione delle regole. Spade/Monete come risorse non implicano *automaticamente* soldati o navi funzionanti: quelli sono entità e transizioni di stato successive.

Un'altra trappola progettuale è il **ciclo delle Monete**. Se si imponessero Monete per costruire l'unica Zecca, senza una scorta iniziale o un'altra fonte raggiungibile, il catalogo sarebbe bloccato. Similmente, richiedere Spade per acquistare la prima Forgia renderebbe irraggiungibile il suo prodotto. La futura analisi di raggiungibilità dovrà provare anche i **primi lotti** con scorte industriali iniziali zero.

## 4. Confini tecnici: che cosa richiede il Core, che cosa no

- **M5.2a/b:** se Spade e Monete vengono approvate come normali risorse, le loro produzioni seguono il passo economico in secondi e la precisione di 0,001 già adottati; nessuna produzione/sottrazione avviene a programma chiuso.
- **Bilanciamento:** tracciare per ogni secondo input disponibili all'inizio, output accreditati a fine passo, addetti attivi, produzione impedita dalla capienza e motivi di fermo. La contesa non può essere stimata sommando le capacità di tutti gli edifici.
- **M5.3/M5.4:** conversione Spade→equipaggiamento di soldati e impiego Monete→Navi richiedono comandi atomici e stati persistenti propri; **non** vanno simulati creando quantità di "edifici Soldato" o "edifici Nave".
- **Salvataggi:** l'approvazione di M5.2 richiederà il percorso **S1/S2** di M5.1, con versionamento esplicito e copia dell'originale. La scelta valuta generale/risorsa determina anche lo schema per gli stati economici futuri.
- **GUI 8-bit:** nelle schede di Forgia/Zecca distinguere *manca Carbone*, *manca Ferro/Oro*, *manca addetto* e *deposito pieno*, sempre con icona **e testo**. Aggiungere schede solo dopo il prototipo civile M4: niente cambio grafico forzato nel checkpoint v21.

## 5. Benchmark richiesti prima di decidere prezzi e ritmi

| Test futuro | Configurazione | Misura o invariante |
| --- | --- | --- |
| **F1: solo Forgia** | 1 filiera ferro, Forgia e scorte metalliche a zero | Primo secondo utile; resa Spade/s a regime; assenza di materiale consumato nel medesimo secondo della sua produzione |
| **F2: solo Zecca** | 1 filiera oro, Zecca e scorte a zero | Primo lotto Monete, stock e capienza; nessuna fonte di Monete obbligatoria per la prima Zecca |
| **F3: ferro+oro senza 2ª miniera carbone** | Due fonderie, Forgia e Zecca, 1 miniera carbone | Contesa riproducibile, arresti esplicati, nessuna risorsa negativa |
| **F4: stessa rete con 2ª miniera carbone** | Come F3, +1 carbone e +1 addetto | Carbone nominale 4/s, output effettivi misurati, costi 130/90 riconciliati |
| **F5: scorte prima, armi/monete dopo** | Carbone unico, fonderie poi Forgia/Zecca | Tempo totale, lavoratori di punta, guadagno/perdita vs F4 e transizioni di ri-assegnazione |
| **F6: fermate e ripresa** | Capienze quasi piene, addetto non assegnato, pause, salvataggio a metà ciclo | Nessuna duplicazione o perdita; forecast e snapshot coerenti |
| **F7: downstream futuro** | Reclutamento e navi soltanto dopo M5.3/M5.4 | Spade/Monete consumate **una sola volta**, rollback su pagamento insufficiente, identità missioni e navi persistente |

Confrontare ciascun profilo su **1.800 secondi** e su almeno una cronologia più lunga; ripetere su regole esattamente identificate. Rendere visibili sia **produttività effettiva**, sia **secondi di blocco per motivo**, sia **tempo per finanziare i nuovi edifici** in una colonia civile sostenuta da Cibo/Pane. Testare sia **A** sia **B** nei prezzi, senza cambiare le ricette a metà benchmark.

## 6. Condizioni di avanzamento

1. **Ora:** discutere e archiviare lo studio; nessuna aggiunta industriale al catalogo o ai salvataggi. La v28 resta a 177 casi xUnit attesi, come il prototipo 8-bit v25; non cambia le ipotesi di questo studio.
2. **Alla prima verifica disponibile:** leggere i benchmark M4 generati da `validate.cmd`, senza trasformare la sola conferma v24 in risultati numerici inventati; validare inoltre il prototipo GUI 8-bit v25 a entrambe le risoluzioni.
3. **Al gate D0:** approvare finalità delle Spade e delle Monete, regola di vittoria, ruolo delle isole e bilancio di soldati/abitanti. Scegliere S1 o S2 prima di introdurre nuove risorse persistenti.
4. **Sviluppo futuro:** implementare/validare M5.1a/b/c e poi M5.2a, M5.2b e M5.2c con test e benchmark dedicati; agganciare Caserma e Cantiere solo ai rispettivi incrementi M5.3 e M5.4.

**Criterio di uscita M5.2:** almeno un percorso riproducibile e salvabile da materiali civili a Ferro/Oro, poi a Spade/Monete; due strategie sul carbone confrontate con misure reali; nessun consumo di oggetti militari o pagamenti monetari ambiguo. Questo dossier **non** dimostra che la durata di otto ore sia raggiungibile: il percorso completo fino alla vittoria resta M6.
