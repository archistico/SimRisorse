# M5.2 — Forgia, Zecca e utilizzo delle risorse industriali

Data: 2026-10-28. **Decisione M5.2 acquisita il 28/10: l'utente ha approvato l'opzione A e l'intero contratto sintetizzato al §3. Non è ancora stata implementata.** La base giocabile resta **`civil-growth-7`**, 17 edifici / 14 risorse, [M5.1b verificata nel progetto locale](../validation/M5_1B_GOLD.md) e ultimo gate dettagliato da 206 test. M4 è chiusa; S1 e parametri A dei cinque edifici di Ferro/Oro erano già scelti. **Il 28/10 l'utente ha confermato «test passati» per la v33**, il cui validatore include il [confronto M5.1c](../validation/M5_1C_INDUSTRY_SCALE.md), ma il log completo non è stato fornito. La realizzazione M5.2 attende la verifica del gate pertinente. Le quantità di Spade per arruolamento e Monete per comandi futuri saranno decise rispettivamente in M5.3/M5.4. I confronti A/B e i numeri teorici seguenti rimangono documentazione storica di prefattibilità, **non** regole già inserite nel gioco.

## 1. Che cosa sappiamo davvero dai riferimenti

Il [diagramma originale](../references/RESOURCE_AND_PRODUCTION_CHAINS.md) collega **Ferro + Carbone → Forgia → Spade** e **Oro + Carbone → Zecca → Monete d'oro**. Mostra inoltre Spade e abitanti verso la Caserma, e Monete con legno lavorato verso il Cantiere navale. Le monete compaiono anche come possibile risultato delle filiere commerciali coloniali. **Non indica quantità, prezzi, dipendenti, tempi, consumi permanenti né il significato delle monete come valuta.**

Tre questioni vanno tenute separate:

1. **Produzione:** Forgia e Zecca trasformano materiali già prodotti in M5.1.
2. **Impiego:** le Spade potrebbero essere consumate dal reclutamento e le Monete dal Cantiere navale; le quantità d'impiego appartengono a M5.3/M5.4 e non sono ancora stabilite.
3. **Economia:** l'eventuale uso delle Monete per acquistare *ogni* edificio sarebbe una modifica generale alla spesa di risorse civili; il riferimento **non** la dimostra. Non introdurla implicitamente.

Proposta di divisione tecnica: **M5.2a Forgia**, poi **M5.2b Zecca**, infine **M5.2c contesa sul carbone, depositi e uso a valle**. Questa sequenza permette di diagnosticare un nuovo consumatore alla volta.

## 2. Ipotesi A: costi e capacità da mettere alla prova

Le due ricette seguenti nacquero come **ipotesi A per il benchmark** e sono state **approvate come contratto di design il 28/10**, ma non sono ancora ricette presenti nel catalogo giocabile:

| Nuovo edificio | Legno lavorato / Pietra | Addetti | Input per addetto/s | Output per addetto/s | Prerequisiti candidati |
| --- | ---: | ---: | --- | --- | --- |
| **Forgia** | **240 / 180** | 1 | 1 Ferro + 1 Carbone | 2 Spade | Fonderia ferro |
| **Zecca** | **210 / 160** | 1 | 1 Oro + 1 Carbone | 3 Monete | Fonderia oro |

Risorse candidate aggiuntive: **Spade** e **Monete**. In un eventuale catalogo approvato M5.1 completo, i conteggi passerebbero da **17 edifici / 14 risorse** a **19 / 16**. Sono proiezioni, non contenuti già presenti. Un riferimento alla Fonderia come prerequisito non significa che questa debba restare sempre attiva: andrà definito se serva il **possesso attuale** o lo **sblocco storico**, secondo la futura politica dei prerequisiti.

L'opzione **B di confronto** conserva lavoratori e ricette A ma applica **1,5× ai soli prezzi delle due nuove strutture**: Forgia **360 / 270**, Zecca **315 / 240**. Per entrambe insieme il sovrapprezzo B è **225 legno lavorato e 170 pietra**. Il benchmark dovrà confrontare il tempo necessario a finanziarle *dopo* M5.1, non solo il costo assoluto. Entrambe le opzioni sono reversibili finché restano numeri su carta.

### Quadratura degli investimenti e della domanda di carbone

Le somme seguenti usano **M5.1 A, ora adottata per Ferro e Oro**, insieme ai prezzi ancora proposti di Forgia/Zecca. Suppongono una sola miniera di ciascun metallo e una sola Forgia e Zecca; gli investimenti che includono questi due edifici restano ipotetici:

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

### Un confronto omogeneo: 240 Spade e 360 Monete

Per rendere concreto il vantaggio della seconda miniera, abbiamo calcolato **un caso teorico** sulla logica attuale di priorità e sul possibile posizionamento di Forgia/Zecca **dopo** le due fonderie nel pianificatore. Le condizioni sono volutamente favorevoli e identiche: tutti gli edifici già acquistati, 1.000 minerali di ciascun metallo già disponibili, 40 abitanti alimentati, addetti sufficienti, depositi non pieni, nessun materiale industriale iniziale, estrazione del Carbone pari a 2/miniera/s. La produzione di un passo è spendibile soltanto dal **secondo successivo**. Ricette e prezzi dei due trasformatori sono quelli della **proposta A**, successivamente approvata per il design ma non ancora implementata.

| Strategia candidata | Investimento totale legno/pietra | Addetti effettivamente assegnati | Prima produzione Spade/Monete | Obiettivo raggiunto al secondo | Stato |
| --- | ---: | ---: | ---: | ---: | --- |
| **Una miniera, lavoro a fasi**: 120 s di fonderie dopo il primo secondo, poi spostare due addetti sui trasformatori per 120 s | **1.300 / 950** | **3**, su 7 posti costruiti | 122 | **241** | Calcolo teorico indipendente, non benchmark .NET |
| **Due miniere, simultaneo**: fonderie in funzione dal secondo 2 e trasformatori dal secondo 3 | **1.430 / 1.040** | **6**, su 8 posti costruiti | 3 | **122** | Calcolo teorico indipendente, non benchmark .NET |
| **Una miniera, tutte le macchine presidiate sempre** | **1.300 / 950** | **5**, su 7 posti costruiti | **Nessuna nei primi 180 s**, con minerali abbondanti | **Non valutato** con scorte a esaurimento | Blocchi successivi delle fonderie possono liberare Carbone |

I posti costruiti includono le Miniere di ferro e d'oro, che qui **non sono presidiate** perché il minerale iniziale è fornito dal checkpoint sintetico. Con i presupposti indicati, **una seconda miniera richiederebbe +130 legno, +90 pietra e un lavoratore**, ma ridurrebbe la durata ideale **di 119 secondi** (241 → 122). Non è una previsione per una partita reale: non include raccolta iniziale, tempi per costruire, alimentazione, capacità, costo-opportunità dei lavoratori o azioni del giocatore. Un benchmark futuro deve verificare il modello in `GameSession` prima di usarlo per il bilanciamento.

**Prova preparata, non ancora eseguita:** il progetto separato [ForgeMintProbe](../../experiments/forge-mint-probe/README.md) costruisce in memoria un catalogo *ipotetico* derivato da `civil-growth-7`, ripete le tre strategie sia con prezzi A sia con B, controlla le scorte secondo per secondo e confronta replay interi/frazionari. Verifica i valori teorici **241/122 s** attraverso le reali operazioni del Core, ma **non è ancora stato compilato né eseguito in .NET**. Rimane fuori dalla soluzione ufficiale, dal validatore e dai salvataggi.

**Estensione v34 (F8/F9, ancora da eseguire):** sei ulteriori controlli ripetono per prezzi A/B i confini dei lotti (a **1.999/2.000 Spade** e **1.998/2.000 Monete** entrambi i produttori devono fermarsi, mentre a **1.998/2.000 e 1.997/2.000** i lotti di 2/3 entrano esattamente) e la precedenza delle fonderie con soli **2 Carbone** disponibili. Il test di 2.000 unità considera **1.000 base + due Magazzini da 500** e verifica l'invariante generale di F8; non approva una diversa capacità di base. Anche questi controlli usano `GameSession`, forecast e replay interi/frazionari senza modificare il gioco.

**Dettaglio importante:** il Carbone prodotto al tempo *t* non viene usato nello stesso passo; con due miniere il primo Ferro/Oro esce al secondo 2 e la prima conversione in Spade/Monete avviene al secondo 3. Se si inserissero Forgia e Zecca *prima* delle fonderie nella priorità, il risultato sarebbe diverso. L'ordine deve quindi essere **esplicito e verificato**, non un effetto collaterale casuale dell'ordine del catalogo.

Con una miniera e tutte le macchine presidiate, le fonderie assorbono inizialmente il Carbone. **Questo non significa che i trasformatori resteranno fermi per sempre**: quando finiscono i minerali o si riempiono i depositi dei metalli, le fonderie possono arrestarsi e liberare combustibile. Serve un benchmark su orizzonte più lungo per quantificare questo comportamento, distinguendo concorrenza stabile e arresti dovuti a monte.

## 3. Scelte economiche che possono cambiare tutto

**Esito del 28/10:** l'utente ha approvato tutte e sei le proposte compatte di questo paragrafo. La tabella delle alternative resta per motivare la decisione; non è più un elenco di scelte sospese.

| Decisione | Opzione semplice scelta il 28/10 | Alternativa non adottata | Perché è importante |
| --- | --- | --- | --- |
| **M5.2-D1: significato Monete** | Risorsa stoccata e consumata da comandi espliciti (es. nave) | Valuta generale dell'economia | La seconda modifica il contratto di **tutti** gli acquisti e la migrazione dei salvataggi |
| **M5.2-D2: destino Spade** | Risorsa consumata quando si recluta | Equipaggiamento durevole assegnato al soldato | Cambia il controllo di duplicazioni, congedi, perdite e salvataggi |
| **M5.2-D3: commercio coloniale** | Futuro produttore alternativo di Monete | Entrata periodica separata dalle scorte | Il Core oggi proietta la capacità della **singola ricetta**; più produttori vanno trattati senza stime ingannevoli |
| **M5.2-D4: carbone** | Estendere la priorità fissa esistente: fonderie Fe/Au prima di Forgia/Zecca | Priorità configurabile dal giocatore | Con una sola miniera e fonderie presidiate i trasformatori possono fermarsi; l'alternativa cambia salvataggi e interfaccia |
| **M5.2-D5: stoccaggio** | Capacità base 1.000 per Spade/Monete, +500 per Magazzino | Capacità o saldo monetario separati | Con ricetta da **3 Monete per passo**, senza uscite la Zecca arriva a **999**, poi si ferma: 1 unità libera non basta per un lotto indivisibile |

**Decisione successiva alla prima stesura:** la colonna semplice è stata scelta esplicitamente dall'utente il 28/10. Le regole M4 su Cibo/Pane, Case e costo delle costruzioni restano valide finché non entra una nuova versione delle regole. Spade/Monete come risorse non implicano *automaticamente* soldati o navi funzionanti: quelli sono entità e transizioni di stato successive.

Un'altra trappola progettuale è il **ciclo delle Monete**. Se si imponessero Monete per costruire l'unica Zecca, senza una scorta iniziale o un'altra fonte raggiungibile, il catalogo sarebbe bloccato. Similmente, richiedere Spade per acquistare la prima Forgia renderebbe irraggiungibile il suo prodotto. La futura analisi di raggiungibilità dovrà provare anche i **primi lotti** con scorte industriali iniziali zero.

### Contratto compatto approvato dall'utente il 28/10

Le seguenti sei decisioni erano originariamente una proposta; **sono tutte approvate come contratto per la futura implementazione M5.2**:

1. **Prezzi A** anziché B, per non aggiungere un secondo ostacolo economico mentre si introduce il personale militare: Forgia **240/180**, Zecca **210/160**, un addetto ciascuna.
2. **Ricette A:** 1 Ferro + 1 Carbone → 2 Spade per secondo; 1 Oro + 1 Carbone → 3 Monete per secondo. Nessun bonus di quantità, nessuna produzione offline.
3. **Spade** come scorta prodotta e *consumata* dal futuro comando di arruolamento; in M5.2 sono solo una risorsa inventariata. La quantità richiesta per soldato è rinviata a M5.3, così come perdite e congedo.
4. **Monete** come scorta per **azioni esplicite** (candidato: acquisto di una nave), non nuova valuta generale. La quantità per nave spetta a M5.4; una futura sorgente coloniale potrà aggiungersi senza rendere impossibile il primo viaggio.
5. **Priorità deterministica** dopo le fonderie: Forgia **100**, Zecca **101**, mantenendo Ferro 90 e Oro 91. L'ordine non sarà modificabile in M5.2: il giocatore può gestire il carbone riassegnando gli addetti o costruendo miniere.
6. **Capacità ordinaria** di Spade e Monete, più l'effetto dei Magazzini esistenti. I lotti devono essere interi rispetto alla ricetta, senza produrre più della capienza per «colmare» un residuo insufficiente.

Questo è il **contratto approvato**, non ancora una funzionalità. Le quantità necessarie per reclutare un soldato o comprare una nave sono **espressamente rinviate** a M5.3/M5.4 e non vanno dedotte dalle ricette produttive. Eventuali varianti successive dovranno essere nuovamente discusse *prima* del codice.

**Quadratura dei prezzi A/B per l'intero apparato (due miniere metalliche e loro fonderie, Forgia, Zecca):**

| Prezzi nuovi trasformatori | Una miniera Carbone | Due miniere Carbone | Differenza B−A |
| --- | ---: | ---: | ---: |
| **A**, ipotesi preferita | 1.300 / 950 | 1.430 / 1.040 | — |
| **B**, solo nuovi prezzi ×1,5 | 1.525 / 1.120 | 1.655 / 1.210 | **+225 / +170** |

Unità: legno lavorato / pietra. **Sono costi aggregati, non un budget che dimostri di poter acquistare tutto dall'inizio della partita**; prerequisiti e capacità possono obbligare a costruzioni ulteriori.

## 4. Confini tecnici: che cosa richiede il Core, che cosa no

- **M5.2a/b:** se Spade e Monete vengono approvate come normali risorse, le loro produzioni seguono il passo economico in secondi e la precisione di 0,001 già adottati; nessuna produzione/sottrazione avviene a programma chiuso.
- **Bilanciamento:** tracciare per ogni secondo input disponibili all'inizio, output accreditati a fine passo, addetti attivi, produzione impedita dalla capienza e motivi di fermo. La contesa non può essere stimata sommando le capacità di tutti gli edifici.
- **M5.3/M5.4:** conversione Spade→equipaggiamento di soldati e impiego Monete→Navi richiedono comandi atomici e stati persistenti propri; **non** vanno simulati creando quantità di "edifici Soldato" o "edifici Nave".
- **Salvataggi:** **S1 è già scelta**. Se Forgia e Zecca entrano **insieme in una sola versione pubblica**, la migrazione candidata è `civil-growth-7 → civil-growth-8`, con entrambe le strutture e risorse a zero. Se invece M5.2a viene distribuita prima di M5.2b, occorrono **due passaggi**: `-7 → -8` per Forgia/Spade e `-8 → -9` per Zecca/Monete. Non aggiungere contenuti a uno stesso ID di regole già salvato. In entrambi i casi mantenere slot, SQLite/snapshot v2, progresso frazionario, lavoratori, tempo e backup alla prima scrittura. Le versioni `-8/-9` sono **candidati**, nessuna è stata introdotta.
- **GUI 8-bit:** nelle schede di Forgia/Zecca distinguere *manca Carbone*, *manca Ferro/Oro*, *manca addetto* e *deposito pieno*, sempre con icona **e testo**. Estendere il layout monospace già approvato, preservando posizione di controlli e comportamento dell'hover.

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
| **F8: lotti e capienza** | Spade a 999/1.000; Monete a 998/1.000 e 999/1.000 | La Forgia da 2/s può arrivare al massimo a 999 se parte da 999 (bloccata); la Zecca da 3/s è bloccata con 1 o 2 posti liberi, senza perdita/credito frazionario |
| **F9: priorità esplicita** | Una miniera, tutte le fonderie/trasformatori presidiati, ordine Fe 90, Au 91, Forgia 100, Zecca 101 | L'ordine dichiarato determina la contesa; il pannello espone la carenza per ciascuna struttura, senza doppia prenotazione di Carbone |
| **F10: compatibilità S1** | Salvare v7, convertire a candidata v8, caricare e salvare; ripetere su v1–v6. Se rilasci separati, verificare anche v8→v9 | Nuovi edifici e risorse partono a zero; preservati scorte, lavoratori, abitanti, case, tempo e frazioni; backup originale, nessuna perdita ai round-trip |

Confrontare ciascun profilo su **1.800 secondi** e su almeno una cronologia più lunga; ripetere su regole esattamente identificate. Rendere visibili sia **produttività effettiva**, sia **secondi di blocco per motivo**, sia **tempo per finanziare i nuovi edifici** in una colonia civile sostenuta da Cibo/Pane. I valori teorici **241/122 secondi** del confronto omogeneo sono *oracoli di controllo* da riprodurre con un checkpoint sintetico e le priorità proposte, non un sostituto della prova da nuova partita. Testare sia **A** sia **B** nei prezzi, senza cambiare le ricette a metà benchmark.

## 6. Condizioni di avanzamento — stato operativo corrente

1. **Baseline acquisita:** M4 chiusa; M5.1a e M5.1b verificate nel progetto locale, `civil-growth-7`, S1, 22 tipi e ruolo della conquista già scelti. Non riaprire le approvazioni precedenti.
2. **Prima del codice M5.2 nel gioco:** la v33 è stata dichiarata passata dall'utente il 28/10; conservare questa conferma ma, se disponibile, acquisire il log completo del gate Windows di **M5.1c** ([comandi](../validation/M5_1C_INDUSTRY_SCALE.md)). L'ultimo rapporto dettagliato da 206 xUnit è precedente al nuovo benchmark; ForgeMintProbe non è compreso nel gate.
3. **Decisioni M5.2 acquisite il 28/10:** prezzi/ricette A, uso mirato delle risorse, priorità 100/101 e deposito ordinario con lotti interi (contratto §3). Restano da decidere **solo nelle rispettive fasi** le quantità d'impiego a valle: M5.3 reclutamento, M5.4 navi.
4. **M5.2a:** quando approvata, introdurre Forgia e Spade con regola/versione dichiarata, test per primo lotto, capienza, carenza e conversione S1. Verificare prima di aggiungere la Zecca.
5. **M5.2b:** introdurre Zecca e Monete, aggiungere test di primo lotto, integrità scorte, salvataggi e presentazione UI. Verificare indipendentemente.
6. **M5.2c:** confronto misurato fra produzione per fasi e parallela (incluso caso 240/360), stress di capienza, riproducibilità dei passi temporali e cronologia da nuova partita. Chiudere solo con artefatti del validatore; niente giudizi sulla durata totale della campagna.

**Criterio di uscita M5.2:** almeno un percorso riproducibile e salvabile da materiali civili a Ferro/Oro, poi a Spade/Monete; due strategie sul carbone confrontate con misure reali; nessun consumo di oggetti militari o pagamenti monetari ambiguo. Questo dossier **non** dimostra che la durata di otto ore sia raggiungibile: il percorso completo fino alla vittoria resta M6.
