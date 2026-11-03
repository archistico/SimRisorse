# M5.4 — decisione sul Cantiere e sulla prima flotta (v51)

**28 ottobre 2026 · Solo progettazione. Nessuna regola navale approvata o implementata.**

Questa scheda aggiorna la [prefattibilità storica](M5_4_NAVAL_PREFEASIBILITY.md) alla baseline **`civil-growth-10`**: **20 tipi di edifici, 16 risorse**, esercito integrato in `GameSession` e SQLite, **276 casi xUnit attesi**. Il gate Windows della correzione v50 è **ancora da confermare**. Il Cantiere e le navi non compaiono nel gioco. Le alternative qui sotto richiedono una scelta dell'utente *prima* di modificare il codice o i salvataggi.

## 1. Vincoli già decisi e fatti verificati nel catalogo

- Prima campagna: **22 tipi di edifici**, tutti necessari per la vittoria; Cantiere e Castello sono gli ultimi due non implementati. **Una conquista obbligatoria** sblocca il Castello.
- **Zecca esistente**: 1 Oro + 1 Carbone → **3 Monete per addetto e secondo simulato**, quando alimentata e assegnata. Le Monete sono una scorta per **pagamenti espliciti**, non la valuta generale delle costruzioni.
- **Segheria esistente**: 2 Tronchi → **1 unità di legno lavorato per addetto e secondo**, se può operare. I costi degli edifici attuali si pagano in legno lavorato e Pietra.
- **Deposito reale**: capacità base **1.000** per Tavole/legno lavorato e Monete; ogni Magazzino aggiunge **500 a ciascuna risorsa**. La Nave, se è un'entità, non deve occupare artificiosamente 1 slot di capacità della risorsa Monete o della risorsa Tavole.
- **Caserma**: 2 posti di addestramento per edificio, 2 Spade consumate per recluta, formazione di gruppi in **60 secondi simulati**. I soldati sono persone della stessa popolazione civile, non una risorsa inventariale fungibile.
- **S1**: un solo percorso di salvataggi, conversioni dichiarate e backup. Demolizione rinviata. Nessun progresso a partita in pausa o chiusa. Nessuna nuova mappa di posizionamento.

## 2. Proposta A — per una prima versione completa ma contenuta

| Decisione | Configurazione **proposta, non approvata** | Perché |
| --- | --- | --- |
| **N-01** Identità | **Navi persistenti**, ognuna con ID univoco, visibili nella Flotta | Evita che una Nave sia trattata come un bene monouso o una capacità immaginaria del Cantiere |
| **N-02** Costruzione Cantiere | **320 Tavole + 220 Pietra**; prerequisito **Zecca**; **0 addetti permanenti** | Sblocca dopo la filiera delle Monete; prezzo superiore a quello della Caserma (280/200), ma sotto le capienze base |
| **N-03** Acquisto nave | **160 Tavole + 120 Monete per nave**; acquisto **immediato** tramite comando, senza un nuovo timer | L'acquisto rende visibile la scelta; il primo lotto usa solo filiere già disponibili e non consuma implicitamente soldati |
| **N-04** Trasporto | **5 soldati addestrati per nave**; nessun equipaggio civile aggiuntivo nella prima campagna | Un ordine da 5 reclute corrisponde a una capienza di nave; l'eventuale contingente richiesto dall'isola resta da definire in M5.5–M5.6 |
| **N-05** Proprietà | Navi **riutilizzabili**, nessuna perdita o ritorno simulato in M5.4 | La prima fase crea una flotta disponibile; partenze e possibili esiti sono regole separate |
| **N-06** Limiti | Nessun limite artificiale di flotta per Cantiere; almeno **un Cantiere presente per acquistare**; navi già pagate non spariscono se in futuro si introduce demolizione | Gli scafi sono beni duraturi, mentre il cantiere è la condizione per costruirne altri |
| **N-07** Navi commerciali | **Rinviate**; non introdurre monete passive o rotte di commercio | Evita una filiera coloniale prima che esistano colonie |

Nel modello A **una Nave non è una 17ª risorsa**: il catalogo passerebbe da **20 a 21 edifici**, mantenendo 16 risorse. Il Castello porterebbe successivamente al totale scelto di 22.

### Alternative da considerare

| Scelta | Variante semplice | Variante più simulativa | Costo di complessità |
| --- | --- | --- | --- |
| Navi | **A, persistenti** | B, monouso a ogni partenza | B è più facile da registrare, ma cancella la gestione della flotta e moltiplica gli acquisti |
| Costruzione nave | **A, immediata** | B, un ordine per Cantiere che matura in **120 s simulati** | B richiede code, prenotazioni di pagamento, avanzamento e persistenza di un secondo orologio |
| Capienza | **A, 5 soldati** | B, 10 soldati | A richiede più scafi per grandi contingenti, B riduce l'importanza della flotta; la difficoltà delle isole non è ancora scelta |
| Costo | **A, 160 Tavole + 120 Monete** | B, 240 Tavole + 180 Monete | B richiede più produzione, ma senza benchmark completi non è dimostrato migliore per l'obiettivo 8 h / 32+ h |

## 3. Contabilità e fattibilità *teorica* della proposta A

Per **un Cantiere e la prima Nave**, dal momento in cui le strutture industriali necessarie esistono, la somma dei nuovi acquisti sarebbe **480 Tavole + 220 Pietra + 120 Monete**. Gli investimenti già necessari per creare Zecca, miniera e fonderia d'oro, carbone, case e produzione di Tavole **non sono inclusi** in questa somma.

La prima Nave richiederebbe **40 secondi teorici** di produzione continua di Monete a 3/s, e **160 secondi teorici** di Segheria a 1/s per le sue 160 Tavole, *se le scorte iniziano da zero*. Non sono durate di gioco misurate: competizione per Carbone, personale, materiali, magazzini e costruzioni precedenti possono rallentarle. Nessun costo individuale supera le capacità base di Tavole e Monete (**1.000**), ma è comunque necessario provare la sequenza reale di acquisti.

La capienza proposta dà:

| Soldati da trasportare | Navi minime `ceil(soldati / 5)` | Solo costo delle navi, oltre al Cantiere |
| ---: | ---: | ---: |
| 1–5 | 1 | 160 Tavole + 120 Monete |
| 6–10 | 2 | 320 Tavole + 240 Monete |
| 11–15 | 3 | 480 Tavole + 360 Monete |

La **forza richiesta per conquistare un'isola** non viene definita da questa tabella: prima M5.5 dovrà stabilirla indipendentemente, verificando che soldati, spade, alloggi e razioni siano raggiungibili. Gli equipaggi non vengono sottratti dalla popolazione se non esiste una regola approvata.

## 4. Confini e incrementi tecnici dopo l'eventuale approvazione

1. **M5.4a — regole e primo acquisto:** aggiungere il Cantiere al catalogo v11, un servizio navale indipendente dal pianificatore di produzione, comando atomico di acquisto e primi test. Conservare una factory storica **v10** per test dei militari e conversioni.
2. **M5.4b — persistenza e GUI:** `ShipId` stabile e monotono, flotta disponibile e salvataggio SQLite nella stessa transazione di risorse/esercito. In continuità con S1, valutare **`civil-growth-11 → -12`** (revisione v53: la v11 è già usata per la capacità uniforme del Pane), snapshot formato 2 con nuova tabella vincolata alla versione, migrazione e backup. Aggiungere scheda **Flotta** con indicazione di prezzo, requisito, navi possedute e motivo del comando disabilitato; test a **1040×740 e 1240×850**.
3. **M5.4c — preflight non distruttivo:** funzione che calcola navi disponibili e capacità teorica, senza assegnare soldati o avviare missioni. I soldati in addestramento non sono trasportabili. Le navi non possono essere conteggiate due volte quando in futuro esisteranno missioni.

**Non fanno parte di M5.4:** esplorazione, isole, avvio/ritorno di missioni, perdite, combattimenti, commercio coloniale, equipaggi persistenti, riparazioni, ripristino di navi e demolizione del Cantiere. Ognuno richiede un contratto successivo. Non introdurre una risorsa `ships` nel catalogo né trasformare la produzione della Zecca in valuta universale.

## 5. Matrice minima di accettazione quando M5.4 sarà implementata

| Categoria | Casi obbligatori |
| --- | --- |
| **Raggiungibilità** | Dalla partita reale, con stock industriali iniziali nulli, ottenere Zecca → Cantiere → prima Nave senza risorse coloniali o valori iniettati |
| **Acquisti** | Un solo clic/ordine consuma le scorte una sola volta; ordine rifiutato con 0 Monete, 119 Monete, 159 Tavole, Cantiere assente o quantità non valida, senza mutazione |
| **Flotta** | Due navi hanno ID distinti, costi riconciliati, capienza totale 10; nessun secondo Cantiere necessario per acquistare la seconda |
| **Militari** | Addestrandi e reclute non contano come soldati pronti; il preflight non arruola, non congeda e non consuma Spade |
| **Salvataggi** | Round-trip v11 con identificatori e stato invariati, riapertura in pausa senza progresso, migrazione v10→v11 con flotta vuota e backup; versioni storiche ancora leggibili |
| **Rifiuti** | Record navali corrotti, duplicate ID, somme fuori limite e stato mancante in una partita v11 rifiutati chiaramente, senza cancellare il file precedente |
| **Interfaccia** | Esercito e Flotta nascosti in `economy-foundation`, icone con testo, tabelle e scrollbar senza sovrapposizione, pulsante ocra su hover e diagnosi dei materiali mancanti |
| **Regressione** | Tutti i **276 casi attuali** continuano a passare; i test aggiuntivi non sono contati fino all'effettiva implementazione |

## 6. Scelta da ricevere

Rispondere **«Approvo M5.4 proposta A»** se prezzi, nave persistente, acquisto immediato, cinque posti e assenza di equipaggio aggiuntivo sono tutti accettabili. In alternativa indicare gli ID da modificare (es. `N-03: costruzione 120 s`, `N-04: 10 soldati`). **Un «continua» generico non viene interpretato come approvazione economica.**

Anche dopo l'approvazione, **prima del nuovo codice** registrare il risultato del `validate.cmd` sulla v50. Se non è ancora disponibile, sono ammesse ulteriori analisi non mutanti e preparazione di test, non il passaggio implicito a una versione giocabile v11.
