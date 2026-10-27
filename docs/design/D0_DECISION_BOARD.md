# SimRisorse — D0: scheda operativa delle decisioni prima di M5

Data: 2026-10-27 · scheda v28 aggiornata in M4.15/v29 · **Scheda di lavoro, non verbale di approvazione.**

Questa pagina serve a prendere le decisioni una alla volta. Il [dossier completo D0](PRE_M5_DECISION_GATE.md) conserva l'inventario dei 43 riferimenti e le alternative ragionate; qui sono raccolti ordine, responsabilità e condizioni per iniziare il codice. Le preferenze tecniche sottostanti **non sono risposte già fornite dall'utente**.

**D0.1 approvata il 2026-10-27:** prima campagna con i **22 tipi scelti** nella [scheda del catalogo e della vittoria](FIRST_CAMPAIGN_PROPOSAL.md), gli altri 21 riferimenti fuori dalla prima campagna. Sono 12 tipi esistenti e 10 nuovi ancora da implementare. Prezzi, ricette, condizione di vittoria e salvataggi non sono approvati da questa scelta.

## 1. Dove siamo davvero

| Elemento | Stato alla v29 | Conseguenza |
| --- | --- | --- |
| M0–M3 | Completate; test confermati nei rispettivi checkpoint | Baseline da preservare |
| M4 fino a v24/M4.10 | **Gate Windows riferito superato dall'utente**, 174 xUnit attesi; nessun TRX/JSON disponibile qui | Ultima conferma utilizzabile |
| v25/M4.11, con correzione PixelGlyph | **Gate Windows verificato: 177/177 test**, nove render ispezionati | Giudizio utente sullo stile e verifica su monitor/DPI reali restano aperti |
| v26/M4.12 | Lettore PowerShell **eseguito**, quattro benchmark consolidati | [Misure e limiti](../validation/M4_MEASURED_RESULTS.md), input conservati nel run |
| v27/M4.13 | Audit matematico delle Case, **non benchmark eseguito** | Confrontare poi con dati reali |
| v28/M4.14 | Questa scheda e allineamento documentale | Gate tecnico verificato; decisioni D0 ancora aperte |
| v29/M4.15 | Prefattibilità navale M5.4; **solo documentazione** | Decisioni e test futuri, nessuna nave introdotta |
| Regole e dati | `civil-growth-5` attiva; snapshot SQLite v2, 12 tipi di edifici e 9 risorse | Non retrocedere alla versione `-4` |
| Catalogo della prima campagna | **22 tipi scelti dall'utente**, di cui 12 implementati e 10 futuri; altri 21 riferimenti fuori dal primo catalogo | Perimetro definito; regole operative e vittoria ancora da fissare |

**Vincoli confermati:** gestione con elenchi/schede, niente posizionamento su mappa; identità visiva pixel art retro 8-bit essenziale; pausa senza progresso offline; la durata di circa 8 ore con strategia efficiente e di 32 ore o più con strategie lente è **un obiettivo**, non un risultato misurato.

## 2. Otto decisioni da discutere, senza assumerle

| ID | Domanda concreta all'utente | Indicazione tecnica **provvisoria** | Blocca / criterio di chiusura |
| --- | --- | --- | --- |
| **D0.1** Catalogo | **Approvata:** i 22 tipi della scheda prima campagna, altri 21 fuori | 12 presenti + 10 futuri; ID nuovi candidati, regole e prezzi da dimensionare | Perimetro scelto; la raggiungibilità quantitativa resta D0.6 |
| **D0.2** Vittoria | Bisogna **possedere**, **aver costruito** o raggiungere una sequenza di obiettivi più un traguardo finale? Che effetto ha la demolizione? | **C: traguardi + obiettivo conclusivo**; possesso al termine da fissare a parte | Cinque scenari positivi/negativi, una sola condizione di vittoria non ambigua |
| **D0.3** Isole | Quante conquiste sono necessarie e quali sblocchi danno? Cosa succede se una spedizione fallisce? | **B: almeno un percorso essenziale**, altre isole facoltative | Prima rotta percorribile senza risorse che si ottengono solo dopo il viaggio |
| **D0.4** Militari | Soldati/reclute occupano case e mangiano? Coda o formazione istantanea? Congedi, perdite e rimborsi? | Unica popolazione contabilizzata; **B: coda** come prima prova, non ancora decisa | `totale = civili impiegati + civili liberi + reclute + soldati`, senza doppioni; casi di pausa/save e transizioni |
| **D0.5** Tempo | Velocità 1:1 o accelerabile? Come contano pause, riprese, livello difficoltà e punteggio? | Cronometrare **solo secondi simulati validi**; scegliere esplicitamente eventuale accelerazione | Cronologie riproducibili, classifica confrontabile, niente avanzamento offline |
| **D0.6** Raggiungibilità | Quale percorso di materiali, addetti, capacità e sblocchi rende possibile il primo obiettivo e la vittoria? | Grafo senza cicli, prova dei primi lotti a scorte nuove zero | Almeno una cronologia fattibile per costi, magazzini, cibo e forza lavoro; precede M5.1 |
| **D0.7** Interfaccia 8-bit | Il prototipo v25 è leggibile a 1040×740 e 1240×850, con DPI reali e messaggi immediati? | Conservare elenchi/schede, pittogramma **più testo**, nessun salto del pulsante Costruisci | Gate Windows e revisione visiva, prima dell'estensione grafica a M5 |
| **D0.8** Spade/Monete | Le **Spade si consumano** all'arruolamento o equipaggiano i soldati? Le **Monete sono risorsa per comandi specifici** o valuta di tutta l'economia? | Primo modello più contenuto da valutare: Spade consumate, Monete come scorta mirata | Contratti di pagamento atomico, annullamento e save; richiesto prima di M5.2–M5.4 |

**Non risolto con questa tabella:** prezzi, ricette, addetti, durata addestramento, rimborso della demolizione, ruolo della Carne, numero delle isole, probabilità di fallimento, uso coloniale delle Monete. Vanno scelti con i dossier specifici, senza inferirli dalle illustrazioni.

## 3. Ordine di approvazione e dipendenze

1. **Chiusura M4:** il gate tecnico è verificato su Windows (**177/177 test**, benchmark, smoke nativi e nove render); il lettore è eseguito e i [risultati misurati](../validation/M4_MEASURED_RESULTS.md) sono distinti dagli [estremi teorici dell'audit case](../scenarios/HOUSING_INCENTIVE_AUDIT.md). Carne e relativi edifici sono fuori dalla prima campagna per D0.1. Restano il giudizio dell'utente sul prototipo e la classificazione della demolizione: approvata, rinviata oppure fuori perimetro. **Solo allora dichiarare conclusa M4.**
2. **Disegno del gioco, D0.1–D0.3:** concordare catalogo, traguardo e funzione delle isole; fissare quali elementi sono opzionali. Si possono discutere **prima** di poter testare, ma l'implementazione resta bloccata.
3. **Contratti trasversali, D0.4, D0.5 e D0.8:** conservazione popolazione, tempo, consumi militari e significato delle Monete. Servono per progettare interfacce e persistenza corrette; non implicano già Caserma, Forgia o Zecca.
4. **Gate di fattibilità, D0.6–D0.7:** completare grafo con costi/capacità, passare almeno un percorso teorico e validare il linguaggio visivo M4. Un dossier pieno di esempi ma senza percorso raggiungibile **non** sblocca M5.
5. **Scelta S1/S2, prima del primo nuovo stato persistente M5.1a:** decidere se proseguire con `civil-growth` o creare una campagna distinta; approvare una strategia di salvataggi e test di migrazione. È una decisione **aggiuntiva e indispensabile**, non una nona D0 implicitamente chiusa.

### Contratto dei salvataggi: non ripetere il numero di versione

| Percorso candidato | Politica proposta | Che cosa richiede per l'approvazione |
| --- | --- | --- |
| **S1 — Estendere `civil-growth`** | La versione corrente è **`civil-growth-5`**. La futura M5 potrebbe usare **`civil-growth-6`**, *se e solo se* la scelta sarà approvata. Snapshot SQLite v2 può restare soltanto se tecnicamente compatibile. | Conversione dichiarata e non ambigua da versioni storiche `-1…-5`, backup originale, test di round-trip e di pausa/ripresa |
| **S2 — Nuovo scenario campagna** | Mantenere lo scenario civile e creare ID distinto; trasferire partite soltanto mediante importazione visibile | Contratto tra identità di scenario, checkpoint, nuova UI e copie originali, senza migrazione silenziosa |

**Oggi vale `civil-growth-5`: `civil-growth-6` non esiste, non è implementata né approvata.** La scelta S1/S2 dipende dall'[analisi M5.1](M5_1_INDUSTRY_PREFEASIBILITY.md) e dal [formato salvataggi](../SAVE_FORMAT.md). Indipendentemente dall'opzione, vecchi file, pause, progressi frazionari e ricompense una tantum vanno protetti.

## 4. Sequenza realizzativa *dopo* le approvazioni

| Fase futura | Aggiunte **candidate**, non approvate | Gate per passare alla successiva |
| --- | --- | --- |
| **M5.1a** | Miniera ferro, miniera carbone, fonderia ferro; minerale ferro, Carbone, Ferro (**+3/+3**) | Costi, ricetta, primo lotto, personale, capacità, backup e SQLite testati; prerequisiti raggiungibili |
| **M5.1b–c** | Oro con miniera/fonderia e 2 risorse (**+2/+2**); confronto della contesa sul carbone | Due filiere riproducibili, benchmark dei prezzi A/B e misura del collo di bottiglia |
| **M5.2a–c** | Forgia, Zecca, Spade, Monete (**+2/+2**), utilizzo da definire | D0.8 approvata, transazioni e produzione a fasi/contemporanea confrontate, mai saldo negativo |
| **M5.3a–c** | Caserma, arruolamento, addestramento, conservazione persone | D0.4 approvata, code deterministiche, nessuna duplicazione su salvataggio/congedo |
| **M5.4** | Cantiere, navi e trasporto: [prefattibilità](M5_4_NAVAL_PREFEASIBILITY.md); costi, riutilizzo e capienza **non approvati** | Primo acquisto raggiungibile, nave salvabile, preflight senza doppio impiego, nessuna missione anticipata |
| **M5.5–M5.7** | Esplorazioni, isole, colonie, castelli e strutture avanzate **solo se scelti** | Missioni a tempo simulato, obiettivi e ricompense idempotenti, scenario completo salvabile |
| **M6** | Vittoria, classifica locale, confronto di strategie complete e bilanciamento | Percorso reale alla vittoria; obiettivi **8 h / 32+ h** valutati con misure, non stimati a priori |
| **M7** | Qualità, accessibilità, regressioni, packaging Windows | Verifica pubblicabile, migrazioni e interfaccia robuste |

Le matrici tecniche estese per [M5.1](M5_1_INDUSTRY_PREFEASIBILITY.md), [M5.2](M5_2_FORGE_MINT_PREFEASIBILITY.md), [M5.3](M5_3_ARMY_PREFEASIBILITY.md) e [M5.4](M5_4_NAVAL_PREFEASIBILITY.md) restano la fonte dei *casi da testare*, non di costi adottati. Gli eventuali **15/12, 17/14 e 19/16** sono conteggi *ipotetici* di edifici/risorse se tutte le proposte precedenti fossero accettate; il Cantiere e le navi sono ulteriori candidati separati.

## 5. Cosa chiedere all'utente quando può rivedere il progetto

- **Prima**, il giudizio dell'utente sui render 8-bit e la classificazione della demolizione; gate Windows e revisione tecnica dei render già completati, Carne fuori dalla prima campagna per D0.1.
- **Poi**, una breve discussione D0.1–D0.3: lista di edifici obbligatori, modello di vittoria e numero di isole necessarie.
- **Successivamente**, D0.4/D0.5/D0.8 e scelta S1/S2, con esempi di missione, arruolamento e salvataggio.
- **Infine**, prezzi A/B e simulazione di raggiungibilità D0.6, prima di scrivere M5.1a.

Il gate tecnico M4 è superato. Prossima scelta D0: valutare la [condizione di vittoria e il ruolo della prima conquista](FIRST_CAMPAIGN_PROPOSAL.md), dopo il catalogo D0.1 già scelto. Restano separati giudizio grafico, demolizione, contratti militari e salvataggi; nessun codice M5 anticipato.
